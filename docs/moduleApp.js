//MODULE PAGE
const moduleMenu = document.querySelector('.module__navbar--menu')
const selectedMenu = document.querySelector('.commands__list--menu')

const displayModule = () => 
{
    let newElemant = document.createElement("div")
    let titleElement = document.createElement("h1")
    let descElement= document.createElement("h2")

    newElemant.className = 'command__list--item'
    titleElement.innerHTML = 'Command'
    descElement.innerHTML = 'Description'

    newElemant.appendChild(titleElement)
    newElemant.appendChild(descElement)

    selectedMenu.appendChild(newElemant)
}

moduleMenu.addEventListener('click', displayModule)