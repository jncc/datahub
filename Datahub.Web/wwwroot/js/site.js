// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function removeKeyword(index) {
    $('#keyword' + index).remove();
    $('#removeKeyword' + index).remove();
    $('form').submit();
}

function isFilteredKeyword(k) {
    var exists = false;

     $('input[name=k]').each((item, element) => {
        if (element.value === k) {
            exists = true;
        }
    });

     return exists;
}

function addKeyword(vocab, value) {
    k = vocab + '/' + value;

     if (! isFilteredKeyword(k)) {
        $('<input>').attr({
            type: 'hidden',
            name: 'k',
            value: k
        }).appendTo('#keywords');

         $('form').submit();
    } 
}
