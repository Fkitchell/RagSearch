import http from 'k6/http'
import { check } from 'k6'

export const options = {
    stages: [
        { duration: '1m', target: 50 }, // simulate ramp-up of traffic from 1 to 50 users over 1 minute.
        { duration: '3m', target: 50 }, // stay at 50 users for 3 minutes
        { duration: '1m', target: 0 }   // ramp-down to 0 users
    ],
    thresholds:{
        http_req_failed: ['rate<0.01'],  // http errors should be less than 1%
        http_req_duration: ['p(95)<500'] // 95% of requests must complete below 500ms
    }
};

export default function () {
const res = http.get(`${__ENV.BASE_URL}/api/health`);
    check(res, {'status is 200': (r) => r.status === 200 });
}