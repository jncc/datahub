// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  get AWS_PROFILE () { return process.env.AWS_PROFILE },
  get AWS_REGION () { return process.env.AWS_REGION },

  get SEARCH_DELETER_LAMBDA () { return process.env.SEARCH_DELETER_LAMBDA },

  get USE_LOCALSTACK () { return process.env.USE_LOCALSTACK.toLowerCase() === 'true' ? true : false }
}
