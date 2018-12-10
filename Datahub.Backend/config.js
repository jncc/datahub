

// wrap up environment variables in one place
// so we're not just accessing process.env everywhere
// note lazy-loading for when these are set in the setup script, potentially after the script is loaded
module.exports = {
  get AWS_REGION() { return process.env.AWS_REGION },
  get ES_ENDPOINT() { return process.env.ES_ENDPOINT },
}
