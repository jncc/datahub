// expose our environment variables in one place
// so we're not just accessing process.env everywhere
export default {
  get AWS_PROFILE () { return process.env.AWS_PROFILE },
  get AWS_REGION () { return process.env.AWS_REGION },

  get SEARCH_DELETER_LAMBDA () { return process.env.SEARCH_DELETER_LAMBDA },
  get HUB_BASE_PATH () { return process.env.HUB_BASE_PATH },

  get USE_LOCALSTACK () { return process.env.USE_LOCALSTACK.toLowerCase() === 'true' }
}
