
import * as dotenv from 'dotenv'

dotenv.config()

// expose our environment variables in one place
// so we're not just accessing process.env everywhere
module.exports = {
  // these are for the search setup tool (to be split out)
  get ES_ENDPOINT() { return process.env.ES_ENDPOINT },
  get ES_INDEX()    { return process.env.ES_INDEX },
  get HUB_URL()     { return process.env.HUB_URL },
  get AWS_PROFILE() { return process.env.AWS_PROFILE },
  get AWS_REGION()  { return process.env.AWS_REGION },

  // these are for the actual backend
  get DYNAMO_TABLE()  { return process.env.DYNAMO_TABLE },
}
