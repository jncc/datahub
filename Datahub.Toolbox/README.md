# Datahub.Toolbox

Currently stores a python script that creates and sends an 'unpublish' message to the lambda ingester. To be triggered manually in Jenkins.

Setup the venv

    python3 -m venv .venv
    source .venv/bin/activate
    pip install -r requirements.txt

Run like so

    python unpublishAsset.py --assetId 2b476591-6a81-4a11-12dd-de7043da9e8c --index test --table datahub-test-assets --function datahub-asset-ingester-test