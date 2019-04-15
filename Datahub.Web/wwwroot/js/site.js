// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
function removeKeyword(vocab, value, buttonId) {
    x = location.search;
    alert(x);
    k=vocab + value;
    $('#' + buttonId).hide();
}
