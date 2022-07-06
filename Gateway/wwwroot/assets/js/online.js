"use strict";

var merchantProductModal = new bootstrap.Modal(document.getElementById('onlineUsersModal'));

function getOnlineUserStatus(lastDate) {

    let minutes = moment().diff(moment(lastDate), 'minutes');

    if (minutes <= 3) {
        //online
        return `<div class="d-flex align-items-center"><div class="bg-success dot rounded-circle me-1"></div><span>Онлайн (${moment(lastDate).locale('ru').fromNow()})</span></div>`
    } else if (minutes > 3 && minutes <= 10) {
        //departed
        return `<div class="d-flex align-items-center"><div class="bg-warning dot rounded-circle me-1"></div><span>Отошел (${moment(lastDate).locale('ru').fromNow()})</span></div>`
    } else if (minutes > 10) {
        //offline
        return `<div class="d-flex align-items-center"><div class="bg-danger dot rounded-circle me-1"></div><span>Оффлайн (${moment(lastDate).locale('ru').fromNow()})</span></div>`
    }
}

function showOnline() {
    $('#onlineUsersTable').DataTable().ajax.reload();
    merchantProductModal.show();
}

$(document).ready(function () {
    $('#onlineUsersTable').DataTable({
        language: { url: '/lib/datatables/ru.json' },
        stateSave: true,
        lengthChange: false,
        order: [[1, 'asc']],
        info: false,
        paging: false,
        searching: false,
        ajax: function (data, callback, settings) {
            APITable(callback,
                '/admin/online',
                { method: 'POST', headers: { 'Content-Type': 'application/json;charset=utf-8' }, body: JSON.stringify(data) });
        },
        columns: [
            { title: "Пользователь", data: "username", className: "fw-bold" },
            { title: "Действие", data: null, className: "fw-bold", render: function (data, type, row, meta) { return `${row.ip} ${row.method} ${row.action}` } },
            { title: "Статус", data: null, render: function (data, type, row, meta) { if (type === 'display') { return getOnlineUserStatus(row.lastDate) } else { return moment().diff(moment(row.lastDate), 'minutes') } } }
        ]
    });

});