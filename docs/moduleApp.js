//MODULE PAGE
const moduleMenu = document.querySelector('.module__navbar--menu')
const selectedMenu = document.querySelector('.commands__list--menu')
const selectedTitle = document.getElementById('selected-header').firstChild
const icon = document.getElementById('general')

var currentModule = ''
var allCommands

function getJson(callback) 
{
    fetch('Commands.json')
    .then(function(response)  
    {
        return response.json();
    })
    .then(function(data)
    {
        allCommands = data

        if(callback)
            callback()
    })    
}

function gatherCommands(id)
{
    let commands = allCommands[id]

    commands.forEach(element => {
        let newElemant = document.createElement("div")
        let titleElement = document.createElement("h1")
        let descElement= document.createElement("h2")
        let brk = document.createElement("br")
    
        newElemant.className = 'command__list--item'
        titleElement.innerHTML = element['name']
        descElement.innerHTML = element['description']
    
        newElemant.appendChild(titleElement)
        newElemant.appendChild(descElement)
    
        selectedMenu.appendChild(newElemant)
        selectedMenu.appendChild(brk)
    });
}

const displayModule = (e) => 
{
    if(!e.target.id || e.target.id === "")
        return;
    else if(currentModule == e.target.id)
        return;

    while(selectedMenu.firstChild)
        selectedMenu.removeChild(selectedMenu.lastChild)

    selectedTitle.innerHTML = e.target.innerHTML === "" ?  document.getElementById(e.target.id).innerHTML : e.target.innerHTML  
    gatherCommands(e.target.id)

    currentModule = e.target.id
}

let index = window.location.href.indexOf("=")
if(index != -1)
    getJson(function()
        {
            currentModule = window.location.href.substring(index + 1, window.location.href.length).replace("%20", " ")
            selectedTitle.innerHTML = document.getElementById(currentModule).innerHTML

            gatherCommands(currentModule)
        })
else 
    getJson()

moduleMenu.addEventListener('click', displayModule)
icon.addEventListener('click', displayModule)