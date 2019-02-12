const fs = require('fs');
const util = require('util');
const readFileAsync = util.promisify(fs.readFile);
const glob = util.promisify(require('glob'));
const env = require('../../../env');
const sendRequest = require('../../sendRequest');
const schema = require('../../schema');
const request = require('request');
const uuidv4 = require('uuid/v4');
const url = require("url");
const pathModule = require("path");

const insertDummyDataWithRemotes = async () => {

    console.log(`Inserting dummy data into index '${env.ES_INDEX}' including data file ingestion...`);
    await insertDummyDocsFromWebProjectWithRemotes();
    console.log(`Inserted dummy data into index '${env.ES_INDEX}'.`);
}

module.exports = insertDummyDataWithRemotes;

// todo: this desparately needs refactoring.
// it's copied-pasted from the original insertDummyDocsFromWebProject command.
// it would be much better to have a flag like --include-data-files
// and unify the two commands.

const insertDummyDocsFromWebProjectWithRemotes = async () => {
    // TODO: Clear existing index with a match all query

    // Get test files to ingest
    let files = await glob('../../../Datahub.Web/Data/**/*.json');
    console.log(`Inserting ${files.length} example files found in Datahub.Web...`);

    // insert each of the dummy entries into the index
    for (let file of files) {
        let doc = JSON.parse(await readFileAsync(file, 'utf8'))
        let path = env.ES_INDEX + '/_doc/' + doc.id + '?pipeline=attachment'

        console.log(`Inserting ${path}...`)

        await sendRequest({
            method: 'PUT',
            path: path,
            body: schema.makeSearchDocumentFromTopcatRecord(doc, env.HUB_URL),
        })

        for (let data of doc.data) {
            if (data.http.type === 'application/pdf') {
                let docRemote = doc;
                // Get Parent title and id before they are overwritten
                docRemote.parent_id = doc.id;
                docRemote.parent_title = doc.metadata.title;
                // Generate new ID and set title to the filename
                docRemote.id = uuidv4();
                docRemote.title = pathModule.basename((url.parse(data.http.url)).pathname);

                var ret = await new Promise((resolve, reject) => {
                    request(data.http.url, {
                        encoding: null
                    }, (error, response, body) => {
                        if (!error && response.statusCode == 200) {
                            docRemote.file_base64 = Buffer.from(body).toString('base64');
                            docRemote.file_size = Buffer.byteLength(body);
                            docRemote.file_extension = 'pdf'
                            path = env.ES_INDEX + '/_doc/' + docRemote.id + '?pipeline=attachment';
                            
                            console.log(`Sending data file to elasticsearch on ${path}...`);
                            sendRequest({
                                    method: 'PUT',
                                    path: path,
                                    body: schema.makeSearchDocumentFromRemote(docRemote, env.HUB_URL)
                                })
                                .then(resp => resolve(resp))
                                .catch(resp => reject(resp))
                        } else {
                            reject(error);
                        }
                    });
                }).catch(resp => console.log(`Error occurred while downloading or uploading remote: ${resp}`));
                console.log(ret);
            }
        }
    }
}