const moduleSection = document.querySelector('.modules__group')

const changeTab = (e) => 
{
    if(!e.target.id || e.target.id === "")
        return;

    window.open("module.html?preLoadModule=".concat(e.target.id), "_self");
}

moduleSection.addEventListener('click', changeTab)