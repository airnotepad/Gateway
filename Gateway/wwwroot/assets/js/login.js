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
        }
    ]
});

function showError(error) {
    if (error != '') {
        notyf.open({
            type: 'error',
            message: getErrorMessage(error)
        });
    }
}

function getErrorMessage(error) {
    switch (error) {
        case 'Incorrect password confirm':
            return 'Повторно пароль введен неверно'
        case 'Incorrect username or password':
            return 'Неправильный логин или пароль'
        case 'Username is not allowed':
            return 'Нельзя использовать такой username'
        case 'Invite is not allowed':
            return 'С приглашением что то не так'
    }
}