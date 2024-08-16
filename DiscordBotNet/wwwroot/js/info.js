let dragging;

function dragStart(event) {

    dragging = event.target;
}

function RemoveBlessing(id) {
    if (id == "") return;
    if (dragging.className != "equipped") return;

    $.ajax({
        type: 'POST',
        url: "/Info/RemoveBlessing/",
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

function SetBlessing(characterId) {
    if (dragging.id == "") return;
    if (dragging.className == "equipped") return;
    let blessingId = dragging.id;
    $.ajax({
        type: 'POST',
        url: "/Info/SetBlessing/",
        contentType: "application/json",
        data: JSON.stringify({blessingId: blessingId, characterId: characterId}),
        success: function (result) {
            location.reload()

            // Handle the result if needed
        },
        error: function (error) {
            console.log(error);
        }
    })

}