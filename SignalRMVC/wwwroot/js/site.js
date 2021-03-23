var createRoomBtn = document.getElementById('create-room-btn')
var createRoomModal = document.getElementById('create-room-modal')

createRoomBtn.addEventListener('click', function() {
    createRoomModal.classList.add('active')
})

var closeRoomModal = function() {
    createRoomModal.classList.remove('active')
}

var membersBtn = document.getElementById('membersBtn')
var membersModal = document.getElementById('members-modal')

membersBtn.addEventListener('click', function () {
    membersModal.classList.add('active')
})

var closeMembersModal = function () {
    membersModal.classList.remove('active')
}