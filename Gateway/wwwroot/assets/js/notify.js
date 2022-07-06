"use strict";

const notyf = new Notyf({
    position: {
        x: 'right',
        y: 'top',
    },
    types: [
        {
            type: 'error',
            background: '#FA5252',
            icon: {
                className: 'fas fa-times',
                tagName: 'span',
                color: '#fff'
            },
            dismissible: false
        },
        {
            type: 'warning',
            background: '#F5B759',
            icon: {
                className: 'fas fa-exclamation-triangle',
                tagName: 'span',
                color: '#fff'
            },
            dismissible: false
        },
        {
            type: 'success',
            background: '#10b981',
            icon: {
                className: 'fas fa-exclamation-triangle',
                tagName: 'span',
                color: '#fff'
            },
            dismissible: false
        }
    ]
});

function showError(error) {
    if (error != '') {
        notyf.open({
            type: 'error',
            message: error
        });
    }
}

function showMessage(message) {
    if (message != '') {
        notyf.open({
            type: 'warning',
            message: message
        });
    }
}

function showSuccess(message) {
    if (message != '') {
        notyf.open({
            type: 'success',
            message: message
        });
    }
}