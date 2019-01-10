
const program = require('yargs')
const createIndex = require('./commands/createIndex')
const deleteIndex = require('./commands/deleteIndex')
const insertDummyData = require('./commands/insertDummyData')
const insertDummyDataWithRemotes = require('./commands/insertDummyDataWithRemotes')

const env = require('../../env')

const main = () => {
  program
    .scriptName('JNCC Datahub & Website ElasticSearch setup tool')
    .usage('$0 <cmd> [args]')
    .option('e', {
      alias: 'endpoint',
      demandOption: true,
      describe: 'The ElasticSearch endpoint URL.',
      type: 'string'
    })
    .option('i', {
      alias: 'index',
      demandOption: true,
      describe: 'The name of the index to use (e.g. main, edit, test).',
      type: 'string'
    })
    .option('aws-region', {
      describe: 'The name of the AWS_REGION to use. Optionally, set an environment variable.',
      type: 'string'
    })
    .option('aws-profile', {
      describe: 'The name of the AWS_PROFILE to use. Optionally, set an environment variable.',
      type: 'string'
    })
    .option('ingest-remotes', {
      describe: 'Attempt to ingest remote files by following links in dummy data structure (http section only)',
      type: 'boolean'
    })
    .command('hello [name]', 'Print the wecome.', (yargs) => {
      yargs.positional('name', {
        type: 'string',
        default: 'Mr. Developer',
        describe: 'The developer to say hello to.'
      })
    }, (argv) => {
      startup(argv)
      console.log(`Hello ${argv.name}, welcome to yargs!`)
    })
    .command('create-index', 'Create the index.', (yargs) => {}, (argv) => {
      startup(argv)
      createIndex()
    })
    .command('delete-index', 'Delete the index.', (yargs) => {}, (argv) => {
      startup(argv)
      deleteIndex()
    })
    .command('insert-dummy-data', 'Insert dummy (pretend) data into the index.', (yargs) => {}, (argv) => {
      startup(argv)
      if (argv.ingestRemotes) {
        insertDummyDataWithRemotes()
      } else {
        insertDummyData()
      }
    })
    .strict()
    .help()
    .argv
}

const startup = (argv) => {   
  console.log(`Welcome to the JNCC Datahub & Website ElasticSearch setup tool.`) 

  process.env['ES_ENDPOINT'] = argv.endpoint
  console.log(`ES_ENDPOINT : ${process.env['ES_ENDPOINT']}`)

  process.env['ES_INDEX'] = argv.index
  console.log(`ES_INDEX    : ${process.env['ES_INDEX']}`)

  if (argv.awsRegion) {
    process.env['AWS_REGION'] = argv.awsRegion
  }
  console.log(`AWS_REGION  : ${process.env['AWS_REGION']}`)

  if (argv.awsProfile) {
    process.env['AWS_PROFILE'] = argv.awsProfile
  }
  console.log(`AWS_PROFILE : ${process.env['AWS_PROFILE']}`)

  if (argv.datahubRoot) {
    process.env['DATAHUB_ROOT'] = argv.awsProfile;
  } else {
    process.env['DATAHUB_ROOT'] = 'https://datahub.jncc.gov.uk';
  }
  console.log(`DATAHUB_ROOT : ${process.env['DATAHUB_ROOT']}`)
}

main()
