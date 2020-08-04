import logging
import argparse

def main(assetId, esIndex, dynamoTable):
    logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(asctime)s: %(message)s')
    logging.info(f'Deleting documents related to asset ID {assetId} from {esIndex} elasticsearch index and {dynamoTable} dynamodb table')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Remove an asset from a specified elasticsearch index and dynamodb table.")
    parser.add_argument('-a', '--assetId', type=str, required=True, help='The asset ID to delete')
    parser.add_argument('-i', '--index', type=str, required=True, help='The elasticsearch index from which to delete the asset documents')
    parser.add_argument('-t', '--table', type=str, required=True, help='The dynamodb table from which to delete the asset document')

    args = parser.parse_args()

    main(args.assetId, args.index, args.table)