
// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  get AWS_PROFILE () { return process.env.AWS_PROFILE },
  get AWS_REGION () { return process.env.AWS_REGIONT },

  get ES_ENDPOINT () { return process.env.ES_ENDPOINT },
  get SQS_ENDPOINT () { return process.env.SQS_ENDPOINT },
  get S3_BUCKET () { return process.env.S3_BUCKET },

  get USE_LOCALSTACK () { return process.env.USE_LOCALSTACK }
}
