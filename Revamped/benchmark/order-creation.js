/**
 * K6 load test: Order Creation
 *
 * - Tests POST /api/orders on Order Service
 * - Uses random X-Created-By GUID each iteration to avoid rate limiter
 *   (RateLimiter: 1 permit per 60s per X-Created-By)
 * - Exports summary report to benchmark/reports/
 *
 * Usage:
 *   k6 run order-creation.js
 *   k6 run --vus 10 --duration 30s order-creation.js
 *
 * Env:
 *   ORDER_SERVICE_BASE_URL  (default: http://localhost:5213)
 *   PRODUCT_SERVICE_BASE_URL (default: http://localhost:5118) - for setup
 *   K6_REPORT_DIR (default: Revamped/benchmark/reports) - report output path
 */

import http from 'k6/http';
import { check, sleep } from 'k6';

const orderBaseUrl = __ENV.ORDER_SERVICE_BASE_URL || 'http://localhost:5213';
const productBaseUrl = __ENV.PRODUCT_SERVICE_BASE_URL || 'http://localhost:5118';

export const options = {
  vus: 5,
  duration: '30s',
  insecureSkipTLSVerify: true,
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.1'],
  },
};

let productIds = [];

export function setup() {
  const res = http.get(`${productBaseUrl}/api/products?pageSize=50`);
  if (!check(res, { 'products fetched': (r) => r.status === 200 })) {
    throw new Error(`Setup failed: cannot fetch products. Status=${res.status}. Ensure Product Service is running with products.`);
  }
  const body = res.json();
  const items = body.items || body.Items || [];
  if (items.length === 0) {
    throw new Error('Setup failed: no products found. Create products first via POST /api/products.');
  }
  productIds = items.map((p) => p.id || p.Id);
  return { productIds };
}

export default function (data) {
  const products = data.productIds;
  if (!products || products.length === 0) return;

  const createdBy = crypto.randomUUID();
  const customerId = crypto.randomUUID();
  const numLines = Math.min(3, Math.max(1, Math.floor(Math.random() * products.length) + 1));
  const selected = [...products].sort(() => Math.random() - 0.5).slice(0, numLines);
  const lines = selected.map((productId) => ({
    productId,
    quantity: Math.floor(Math.random() * 3) + 1,
  }));

  const payload = JSON.stringify({
    customerId,
    lines,
  });

  const res = http.post(`${orderBaseUrl}/api/orders`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'X-Created-By': createdBy,
    },
  });

  check(res, {
    'order created 201': (r) => r.status === 201,
  });

  if (res.status !== 201) {
    console.warn(`Order creation failed: ${res.status} - ${res.body}`);
  }

  sleep(0.5 + Math.random());
}

export function handleSummary(data) {
  const reportDir = __ENV.K6_REPORT_DIR || 'Revamped/benchmark/reports';
  return {
    [`${reportDir}/summary.json`]: JSON.stringify(data, null, 2),
    [`${reportDir}/summary.html`]: htmlReport(data),
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
  };
}

function textSummary(data, opts) {
  const s = data.metrics;
  const lines = [
    '',
    '====================',
    '  Order Creation K6',
    '====================',
    `  iterations: ${s.iterations?.values?.count ?? 0}`,
    `  http_reqs:   ${s.http_reqs?.values?.count ?? 0}`,
    `  failed:      ${(s.http_req_failed?.values?.rate ?? 0) * 100}%`,
    `  p95 (ms):    ${s.http_req_duration?.values?.['p(95)'] ?? 'n/a'}`,
    '====================',
  ];
  return lines.join('\n');
}

function htmlReport(data) {
  const m = data.metrics || {};
  const iterations = m.iterations?.values?.count ?? 0;
  const httpReqs = m.http_reqs?.values?.count ?? 0;
  const failedRate = (m.http_req_failed?.values?.rate ?? 0) * 100;
  const p95 = m.http_req_duration?.values?.['p(95)'] ?? 0;
  const avgDuration = m.http_req_duration?.values?.avg ?? 0;

  return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Order Creation - K6 Report</title>
  <style>
    body { font-family: system-ui, sans-serif; max-width: 600px; margin: 2rem auto; padding: 1rem; }
    table { border-collapse: collapse; width: 100%; margin-top: 1rem; }
    th, td { border: 1px solid #ddd; padding: 0.5rem 1rem; text-align: left; }
    th { background: #f5f5f5; }
    .ok { color: green; }
    .fail { color: red; }
  </style>
</head>
<body>
  <h1>Order Creation Load Test</h1>
  <p><strong>Generated:</strong> ${new Date().toISOString()}</p>
  <table>
    <tr><th>Metric</th><th>Value</th></tr>
    <tr><td>Iterations</td><td>${iterations}</td></tr>
    <tr><td>HTTP Requests</td><td>${httpReqs}</td></tr>
    <tr><td>Failed Rate</td><td class="${failedRate < 10 ? 'ok' : 'fail'}">${failedRate.toFixed(2)}%</td></tr>
    <tr><td>Avg Duration (ms)</td><td>${typeof avgDuration === 'number' ? avgDuration.toFixed(2) : avgDuration}</td></tr>
    <tr><td>p95 Duration (ms)</td><td>${typeof p95 === 'number' ? p95.toFixed(2) : p95}</td></tr>
  </table>
</body>
</html>`;
}
