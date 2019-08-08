// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  get AWS_PROFILE () { return process.env.AWS_PROFILE },
  get AWS_REGION () { return process.env.AWS_REGION },

  get ES_ENDPOINT () { return process.env.ES_ENDPOINT },

  get USE_LOCALSTACK () { return process.env.USE_LOCALSTACK }
}
