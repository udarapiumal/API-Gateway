import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

const passed  = new Counter('requests_passed');
const blocked = new Counter('requests_blocked');
const latency = new Trend('request_latency');

export const options = {
  scenarios: {
    // Test 1: steady load
    steady_load: {
      executor: 'constant-vus',
      vus: 10,
      duration: '30s',
    },
  },
};

export default function () {
  const res = http.get('https://localhost:7097/products', {
    headers: {
      'X-Api-Key': '550e8400-e29b-41d4-a716-446655440000',
    },
    // ignore self-signed cert in dev
    insecureSkipTLSVerify: true,
  });

  latency.add(res.timings.duration);

  if (res.status === 200) passed.add(1);
  if (res.status === 429) blocked.add(1);

  check(res, {
    'status is 200 or 429': (r) => r.status === 200 || r.status === 429,
    'no 500 errors':        (r) => r.status !== 500,
    'response under 200ms': (r) => r.timings.duration < 200,
  });

  sleep(0.1);
}