# Back End Website Updater

This was a small tool created out of my annoyance at having to go through my code and figure out what I was sending my website APIs every time I wanted to update my website. This really made no sense as the whole idea of using node back end APIs was to not have to go through my code every time I needed to update. With this tool I can now do that. While this application is tooled towards my website it is dynamic in the sense that an pages and page types are set within the Settings.json file. This means that anyone who wants to take the [back end for my personal website](https://github.com/alistairfink/Personal-Website-V3) and expand upon can also use this tool as none of my pages are hard coded. 

I created this tool using Microsoft's WPF framework as a method of increasing my familiarity with C# and .Net as a whole. 



## Types of Pages

There are three types of pages and these correspond to how my personal website's back end handles content.

The first type of page are called "Single" type pages and represents pages with only one set of fields. An example of this is my "About Me" page which only includes my name, description, picture, etc. As there can never be another instance an add function is not required. Instead only the update function is required.

<p align="center">
    <img src="./images/1.png"/>
</p>

The second type of page is called "Array" and represents pages in which multiple instances of the page content can be created something like my education page or experience page is an example of this. These pages are returned as simple arrays of objects from my back end APIs.

The last type of page is called "Listed" type pages. These are very similar to the "Array" type pages with the simple difference that the initial get call returns a barebones list of the actual contents. Another call is required in order to get the entire object. This pattern is demonstrated in my portfolio page in which initially only the barebones list is required. When a user clicks a portfolio item a separate call is then processed in which the actual item is retrieved.

<p>
    <img src="./images/2.png"/>
</p>

<p>
    <img src="./images/3.png"/>
</p>