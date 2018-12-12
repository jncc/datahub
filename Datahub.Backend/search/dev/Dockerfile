
# Creates a dev-time ElasticSearch docker image with the ingest-attachment plugin installed.
# We use this plugin for indexing PDFs etc.
# It's pre-installed on AWS ESS.

FROM docker.elastic.co/elasticsearch/elasticsearch:6.4.2
RUN cd /usr/share/elasticsearch \
  && bin/elasticsearch-plugin install -b -s ingest-attachment
