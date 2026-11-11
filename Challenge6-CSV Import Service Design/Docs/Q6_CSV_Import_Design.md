I will outline the endpoints first so that we can understand the architecture better.

Endpoints:

1. POST - /api/v1/imports/jobs:
Accepts file upload, creates a job_id, saves raw files to Blob storage, and queues a message and it should return 202 Accepted as http status code.

2. GET - /api/v1/imports/jobs/{jobId}:
Returns Status (QUEUED, PROCESSSING, COMPLETED, FAILED), percent complete and row counts.

3. POST - /api/v1/imports/jobs/{jobId}/cancel:
Cancels token in background processor

4. GET - /api/v1/imports/jobs/{jobId}/error:
Streams error report CSV(Row, Column, ErrorMessage).

Execution Flow:
1. A background worker(for example., xxure Service bus) streams CSV line by line usin gCSV helper  without loading 100000 rows into memory.

2. Batches of 1000 validated rows are written via SQLBulkCopy(SQL SERVER)

3. Validation errors are written to an ImportErrors table.

MVP vs Later Iteration:

1. Build Now:
Single worker instance, stream-based validation, DB-backed progress counters, direct download endpoint for errors.

2. Build Later:
Chunked parallel processing across multiple worker nodes, client pre-signed direct upload to BLOB storage, and generating error reports directly to cloud storage buckets.