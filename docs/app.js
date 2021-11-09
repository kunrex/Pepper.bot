//INDEX PAGE
const mobileMenu = document.querySelector('#mobile-menu')
const menuLink = document.querySelector('.navbar__menu')
const navLogo = document.querySelector('#navbar__logo')
const moduleSection = document.querySelector('.modules__group')

//Mobile Menu
const mobileMenuAnimation = () => 
{
    mobileMenu.classList.toggle('is-active')
    menuLink.classList.toggle('active')
}

mobileMenu.addEventListener('click', mobileMenuAnimation)

//Menu disablng
const mobileMenuDisable = () => 
{
    const menuBars = document.querySelector('.is-active')

    if(window.innerWidth <= 960 && menuBars)
    {
        mobileMenu.classList.toggle('is-active')
        menuLink.classList.remove('active')
    }
}

menuLink.addEventListener('click', mobileMenuDisable)
navLogo.addEventListener('click', mobileMenuDisable)

const changeTab = (e) => 
{
    if(!e.target.id || e.target.id === "")
        return;

    window.open("module.html?currentModule=".concat(e.target.id), "_self");
}

moduleSection.addEventListener('click', changeTab)