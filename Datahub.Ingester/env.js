// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  get AWS_PROFILE () { return process.env.AWS_PROFILE },
  get AWS_REGION () { return process.env.AWS_REGION },

  // get LAMBDA_BUCKET () { return process.env.LAMBDA_BUCKET },

  get ES_ENDPOINT () { return process.env.ES_ENDPOINT },
  // get ES_INDEX () { return process.env.ES_INDEX },
  // get ES_SITE () { return process.env.ES_SITE },

  // get SQS_ENDPOINT () { return process.env.SQS_ENDPOINT },
  // get SQS_BUCKET () { return process.env.SQS_BUCKET },

  // get DYNAMO_TABLE () { return process.env.DYNAMO_TABLE },

  // get HUB_BASE_URL () { return process.env.HUB_BASE_URL },

  get USE_LOCALSTACK () { return process.env.USE_LOCALSTACK.toLowerCase() === 'true' ? true : false }
}
