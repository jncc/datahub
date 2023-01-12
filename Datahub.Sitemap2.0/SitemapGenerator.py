import boto3
from collections import namedtuple
import os
import Parameters
from urllib.parse import urlunparse
from xml.dom import minidom

SITEMAPS_URL = 'http://www.sitemaps.org/schemas/sitemap/0.9'
TEMPORARY_FILENAME = 'sitemap-temp.xml'

Components = namedtuple(
    typename='Components', 
    field_names=['scheme', 'netloc', 'url', 'path', 'query', 'fragment']
)

def add_text_node(root, parent_node, node_name, node_value):
    node = root.createElement(node_name)
    parent_node.appendChild(node)
    text = root.createTextNode(node_value)
    node.appendChild(text)

def get_items_from_db():
    dynamodb = boto3.resource('dynamodb')
    table = dynamodb.Table(Parameters.parameters['table'])
    response = table.scan(
        AttributesToGet=['id', 'timestamp_utc']
    )
    items = response['Items']

    while 'LastEvaluatedKey' in response:   # Paging code required due to limit on data size returned from API
        response = table.scan(
            AttributesToGet=['id', 'timestamp_utc'],
            ExclusiveStartKey=response['LastEvaluatedKey'])
        items.extend(response['Items'])

    return items

def create_xml(items):
    root = minidom.Document()
    urlset = root.createElement('urlset') 
    urlset.setAttribute('xmlns', SITEMAPS_URL)
    root.appendChild(urlset)

    for item in items:
        location = urlunparse(
            Components(
                scheme=Parameters.parameters['scheme'],
                netloc=Parameters.parameters['host'],
                path='',
                url='{}/{}'.format(Parameters.parameters['base_path'] or '', item['id']),
                query='',
                fragment=''
            )
        )
        url = root.createElement('url')
        urlset.appendChild(url)
        add_text_node(root, url, 'loc', location)
        add_text_node(root, url, 'lastmod', item['timestamp_utc'])
        add_text_node(root, url, 'changefreq', Parameters.parameters['change_frequency'])

    return root

def write_temporary_file(root):
    xml_str = root.toprettyxml(indent="  ") 
    with open(TEMPORARY_FILENAME, "w") as f:
        f.write(xml_str)

def upload_temporary_file_to_s3():
    s3 = boto3.resource(service_name = 's3')
    s3.meta.client.upload_file(Filename = TEMPORARY_FILENAME, Bucket = Parameters.parameters['bucket'], Key = Parameters.parameters['key'])

items = get_items_from_db()
root = create_xml(items)
write_temporary_file(root)
upload_temporary_file_to_s3()
os.remove(TEMPORARY_FILENAME)
