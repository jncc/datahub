/* eslint-disable no-unused-expressions */
import { exec } from 'child_process'
import { default as axios } from 'axios';
import urljoin from 'url-join'
import { scriptName } from 'yargs'
import { readFileSync } from 'fs'
import { v4 as uuid4 } from 'uuid'

const main = async () => {
  scriptName('JNCC Datahub Ingester localstack setup')
    .usage('$0 <cmd> [args]')
    .option('i', {
      alias: 'index',
      describe: 'The name of the index to use.',
      default: 'website-search-dev',
      type: 'string'
    })
    .option('t', {
      alias: 'table',
      describe: 'The name of the table to use.',
      default: 'datahub-assets-dev',
      type: 'string'
    })
    .option('q', {
      alias: 'queue',
      describe: 'The name of the queue to use.',
      default: 'datahub-queue-dev',
      type: 'string'
    })
    .option('b', {
      alias: 'bucket',
      describe: 'The name of the bucket to use.',
      default: 'datahub-queue-assets-dev',
      type: 'string'
    })
    .command('create-stack', 'Sets up the localstack from scratch', (yargs) => { }, async (argv) => {
      await createDynamoDB(argv.table)
      await createSQSQueue(argv.queue)
      await createS3Bucket(argv.bucket)
      await createSearchIndex(argv.index)
    })
    .command('delete-stack', 'Deletes the localstack from scratch', (yargs) => { }, async (argv) => {
      await deleteDynamoDB(argv.table)
      await deleteSQSQueue(argv.queue)
      await deleteS3Bucket(argv.bucket)
      await deleteSearchIndex(argv.index)
    })
    .command('reset-stack', 'Resets the stack to its initial state', (yargs) => { }, async (argv) => {
      await clearDynamoDB(argv.table)
      await clearSQSQueue(argv.queue)
      await clearS3Bucket(argv.bucket)
      await clearSearchIndex(argv.index)
    })
    .command('create-search-index', 'Create the index.', (yargs) => { }, async (argv) => {
      await createSearchIndex(argv.index)
    })
    .command('delete-search-index', 'Delete the index.', (yargs) => { }, async (argv) => {
      await deleteSearchIndex(argv.index)
    })
    .command('clear-search-index', 'Clear the search index.', (yargs) => { }, async (argv) => {
      await clearSearchIndex(argv.index)
    })
    .command('insert-search-index-dummy-data', 'Insert a set of dummy records into dynamo for testing', (yargs) => { }, async (argv) => {
      await insertSearchIndexDummyData(argv.index)
    })
    .command('create-dynamodb', 'Create the dynamodb table.', (yargs) => { }, async (argv) => {
      await createDynamoDB(argv.table)
    })
    .command('delete-dynamodb', 'Delete the dynamodb table.', (yargs) => { }, async (argv) => {
      await deleteDynamoDB(argv.table)
    })
    .command('clear-dynamodb', 'Clear the dynamodb table.', (yargs) => { }, async (argv) => {
      await clearDynamoDB(argv.table)
    })
    .command('create-s3', 'Create the S3 Bucket.', (yargs) => { }, async (argv) => {
      await createS3Bucket(argv.bucket)
    })
    .command('delete-s3', 'Delete the S3 Bucket.', (yargs) => { }, async (argv) => {
      await deleteS3Bucket(argv.bucket)
    })
    .command('clear-s3', 'Clear the S3 Bucket.', (yargs) => { }, async (argv) => {
      await clearS3Bucket(argv.bucket)
    })
    .command('create-sqs', 'Create the SQS queue.', (yargs) => { }, async (argv) => {
      await createSQSQueue(argv.queue)
    })
    .command('delete-sqs', 'Delete the SQS queue.', (yargs) => { }, async (argv) => {
      await deleteSQSQueue(argv.queue)
    })
    .command('clear-sqs', 'Clear the SQS queue.', (yargs) => { }, async (argv) => {
      await clearSQSQueue(argv.queue)
    })
    .strict()
    .help()
    .argv
}

function createDynamoDB (table) {
  console.log(`CREATE DynamoDB Table - ${table}`)
  runCommand(`aws dynamodb --endpoint http://localhost:4569 create-table --table-name ${table} --attribute-definitions "AttributeName=id,AttributeType=S" --key-schema "AttributeName=id,KeyType=HASH" --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1`)
}

function deleteDynamoDB (table) {
  console.log(`DELETE DynamoDB Table - ${table}`)
  runCommand(`aws dynamodb --endpoint http://localhost:4569 delete-table --table-name ${table}`)
}

function clearDynamoDB (table) {
  deleteDynamoDB(table)
  createDynamoDB(table)
}

function createSQSQueue (queue) {
  console.log(`CREATE SQS Queue - ${queue}`)
  runCommand(`aws sqs --endpoint http://localhost:4576 create-queue --queue-name ${queue}`)
}

function deleteSQSQueue (queue) {
  console.log(`DELETE SQS Queue - ${queue}`)
  runCommand(`aws sqs --endpoint http://localhost:4576 delete-queue --queue-url http://localhost:4576/queue/${queue}`)
}

function clearSQSQueue (queue) {
  deleteSQSQueue(queue)
  createSQSQueue(queue)
}

function createS3Bucket (bucket) {
  console.log(`CREATE S3 Bucket - ${bucket}`)
  runCommand(`aws s3api --endpoint http://localhost:4572 create-bucket --bucket ${bucket}`)
}

function deleteS3Bucket (bucket) {
  console.log(`DELETE S3 Bucket Contents - ${bucket}`)
  runCommand(`aws s3 --endpoint http://localhost:4572 rm s3://${bucket}/ --recursive`)
  console.log(`DELETE S3 Bucket - ${bucket}`)
  runCommand(`aws s3api --endpoint http://localhost:4572 delete-bucket --bucket ${bucket}`)
}

function clearS3Bucket (bucket) {
  runCommand(`aws s3 --endpoint http://localhost:4572 rm s3://${bucket}/ --recursive`)
}

async function createSearchIndex (index) {
  console.log(`CREATE Search Index - ${index}`)
  await axios.put(urljoin('http://localhost:4571', index))
  console.log(`CREATE Search Index Mapping - ${index}`)
  await axios.put(urljoin('http://localhost:4571', index, '_mapping', '_doc'), {
    properties: {
      site: { type: 'keyword' },
      title: { type: 'text' },
      content: { type: 'text', term_vector: 'with_positions_offsets' },
      content_truncated: { type: 'keyword', index: false },
      url: { type: 'keyword' },
      keywords: {
        type: 'nested',
        properties: {
          vocab: { type: 'keyword' },
          value: { type: 'keyword' }
        }
      },
      resource_type: { type: 'keyword' },
      published_date: { type: 'date' },
      parent_id: { type: 'keyword' },
      parent_title: { type: 'text' },
      parent_resource_type: { type: 'keyword' },
      file_extension: { type: 'keyword' },
      file_bytes: { type: 'long', index: false },
      footprint: { type: 'geo_shape' },
      timestamp_utc: { type: 'date' }
    }
  })
}

async function deleteSearchIndex (index) {
  console.log(`DELETE Search Index - ${index}`)
  await axios.delete(urljoin('http://localhost:4571', index))
}

async function insertSearchIndexDummyData (index) {
  console.log(`INSERT dummy data into index - ${index}`)
  var event = JSON.parse(readFileSync('../event.json'))
  await axios.put(urljoin('http://localhost:4571', index, '_doc', event.asset.id), {
    site: 'datahub'
  })
  await axios.put(urljoin('http://localhost:4571', index, '_doc', uuid4()), {
    site: 'datahub',
    parent_id: event.asset.id
  })
}

async function clearSearchIndex (index) {
  await deleteSearchIndex(index)
  await createSearchIndex(index)
}

async function runCommand (cmd) {
  await exec(cmd, (err, stdout, stderr) => {
    if (err) {
      console.error(err)
      return false
    }
  })
  return true
}

main()
