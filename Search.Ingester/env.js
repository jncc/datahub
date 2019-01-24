
// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  get ES_PROTOCOL() { return process.env.ES_PROTOCOL },
  get ES_HOSTNAME() { return process.env.ES_HOSTNAME },
  get AWS_PROFILE() { return process.env.AWS_PROFILE },
  get AWS_REGION()  { return process.env.AWS_REGION },
}
