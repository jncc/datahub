import logging
import argparse
import json
import boto3

def main(assetId, esIndex, dynamoTable, functionName):
    logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(asctime)s: %(message)s')
    logging.info(f'Invoking lambda {functionName} to delete documents related to asset ID {assetId} from {esIndex} Opensearch index and {dynamoTable} dynamodb table')

    message = {
        "config": {
            "elasticsearch": {
                "index": esIndex,
                "site": "datahub"
            },
            "dynamo": {
                "table": dynamoTable
            },
            "action": "unpublish"
        },
        "asset": {
            "id": assetId
        }
    }

    logging.info(f'Message to send: {json.dumps(message)}')

    client = boto3.client('lambda')
    response = client.invoke(
        FunctionName=functionName,
        Payload=json.dumps(message)
    )

    logging.info(f'Got response: {response}')
    if response and response['StatusCode'] == 200 and not 'FunctionError' in response:
        logging.info(f'Successfully unpublished asset {assetId}')
    else:
        raise Exception(f'Error unpublishing asset {assetId}, check the cloudwatch logs for lambda function {functionName}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Remove an asset from a specified elasticsearch index and dynamodb table.")
    parser.add_argument('-a', '--assetId', type=str, required=True, help='The asset ID to delete')
    parser.add_argument('-i', '--index', type=str, required=True, help='The elasticsearch index from which to delete the asset documents')
    parser.add_argument('-t', '--table', type=str, required=True, help='The dynamodb table from which to delete the asset document')
    parser.add_argument('-f', '--function', type=str, required=True, help='The lambda function to invoke')

    args = parser.parse_args()

    main(args.assetId, args.index, args.table, args.function)