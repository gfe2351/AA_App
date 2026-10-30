document.addEventListener('DOMContentLoaded', function () {
    var body = document.getElementById('rows');
    var tpl = document.getElementById('rowTpl');
    var add = document.getElementById('addRow');
    if (!body || !tpl || !add) return;

    add.addEventListener('click', function () {
        body.appendChild(tpl.content.cloneNode(true));
    });

    body.addEventListener('click', function (e) {
        if (e.target.classList.contains('del')) {
            e.target.closest('tr').remove();
        }
    });
});
