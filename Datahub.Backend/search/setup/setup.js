
require('dotenv').config()

const program = require('yargs')
const createIndex = require('./commands/createIndex')
const deleteIndex = require('./commands/deleteIndex')
const insertDevData = require('./commands/insertDevData')

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
      describe: 'The name of the index to use (main or edit).',
      type: 'string'
    })
    .command('hello [name]', 'Print the wecome.', (yargs) => {
      yargs.positional('name', {
        type: 'string',
        default: 'Pete',
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
    .command('insert-dev-data', 'Insert dev data into the index.', (yargs) => {}, (argv) => {
      startup(argv)
      insertDevData()
    })
    .strict()
    .help()
    .argv
}

const startup = (argv) => {   
  console.log(`Welcome to the JNCC Datahub & Website ElasticSearch setup tool.`) 
  process.env['AWS_REGION'] = argv.awsRegion
  process.env['ES_ENDPOINT'] = argv.endpoint
  console.log(`AWS_REGION : ${process.env['AWS_REGION']}`)
  console.log(`ES_ENDPOINT  : ${process.env['ES_ENDPOINT']}`)
}

main()
