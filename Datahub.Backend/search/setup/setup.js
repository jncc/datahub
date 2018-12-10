
const program = require('yargs')
const createIndex = require('./createIndex')

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
    .option('r', {
      alias: 'awsRegion',
      default: 'eu-west-1',
      describe: 'The AWS region.'
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
    .command('create-index', 'Create the index (normally a one-off thing).', (yargs) => {}, (argv) => {
      startup(argv)
      createIndex()
    })
    .strict()
    .help()
    .argv
  
    // create-index createIndex()
    // populate-dev-data
}

const startup = (argv) => {   
  console.log(`Welcome to the JNCC Datahub & Website ElasticSearch setup tool.`) 
  process.env['AWS_REGION'] = argv.awsRegion
  process.env['ES_ENDPOINT'] = argv.endpoint
  console.log(`AWS_REGION : ${process.env['AWS_REGION']}`)
  console.log(`ES_ENDPOINT  : ${process.env['ES_ENDPOINT']}`)
}

main()
