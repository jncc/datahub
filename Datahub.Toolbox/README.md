# Datahub.Toolbox

Currently stores a python script that creates and sends an 'unpublish' message to the lambda ingester. To be triggered manually in Jenkins.

    python unpublishAsset.py --assetId 2b476591-6a81-4a11-12dd-de7043da9e8c --index test --table dev