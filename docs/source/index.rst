.. CoreSharp documentation master file, created by
   sphinx-quickstart on Tue Nov 10 01:19:14 2020.
   You can adapt this file completely to your liking, but it should at least
   contain the root `toctree` directive.

Welcome to CoreSharp's documentation!
=====================================


Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer mollis, libero non rhoncus ultricies, ex velit tristique odio, ac tristique enim nibh ac arcu. 
Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Morbi sapien erat, maximus id malesuada sit amet, scelerisque sit amet ipsum. 
Nunc congue et arcu nec rutrum. In porta ex a dapibus vehicula. Cras pellentesque justo urna, eget condimentum felis interdum a. Duis volutpat eu neque a pulvinar. 
Vivamus cursus massa et tempor viverra. Ut rhoncus lacinia nisl, placerat pretium diam lobortis non. Morbi quis fermentum elit. Nullam ultricies efficitur tellus, ac dapibus nibh sagittis vel. 
Aliquam sem neque, molestie sit amet euismod a, lobortis nec ante.

Installation
============

Get official builds from NuGet or run the following command in the Package Manager Console: 

`PM> Install-Package CoreSharp.Common`

Packages
========

* :doc:`common` - Common stuff for all other packages.
* :doc:`breeze` - Breeze integration package
* :doc:`data-access` - Common data access interfaces, attributes, extensions etc.
* :doc:`cqrs` - Command Query Responsibility Segregation.
* :doc:`graphql` - GraphQL schema generation from CQRS.
* :doc:`identity` - Asp.Net Core Identity implementation.
* :doc:`nhibernate` - NHibernate conventions, unit of work, db store, base entity, versioned entity etc.
* :doc:`validation` - FluentValidation extension.

Contents:

.. toctree::
   :maxdepth: 3

   common
   breeze
   data-access
   cqrs
   graphql
   identity
   nhibernate
   validation
   release-notes

Indices and tables
==================

* :ref:`genindex`
* :ref:`modindex`
* :ref:`search`
