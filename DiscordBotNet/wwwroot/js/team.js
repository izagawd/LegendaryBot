let draggedElement

function dragStart(event) {

    draggedElement = event.target;
}


function RemoveFromTeam(event) {

    if (draggedElement.className.includes("character_in_list_div")) return;

    let id = draggedElement.id;
    $.ajax({
        type: 'POST',
        url: "Team/RemoveFromTeam/",
        contentType: "application/json",
        data: JSON.stringify(id),
        success: function (result) {
            location.reload()
            // Handle the result if needed
        },
        error: function (error) {
            console.log(error);
        }
    })
}

function AddToTeam(event) {

    if (!draggedElement.className.includes("character_in_list_div")) return;

    let id = draggedElement.id;

    $.ajax({
        type: 'POST',
        url: "Team/AddToTeam/",
        contentType: "application/json",
        data: JSON.stringify(id),
        success: function (result) {
            location.reload()

            // Handle the result if needed
        },
        error: function (error) {
            console.log(error);
        }
    })

}