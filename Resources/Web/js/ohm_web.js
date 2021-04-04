/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>

*/

ko.bindingHandlers.treeTable = {
  update: function(element, valueAccessor, allBindingsAccessor) {
    var dependency = ko.utils.unwrapObservable(valueAccessor()),
    options = ko.toJS(allBindingsAccessor().treeOptions || {});

    setTimeout(function() { $(element).treeTable(options); }, 0);
  } 
};

var node = function(config, parent) {
  this.parent = parent;
  var _this = this;

  var mappingOptions = {
    Children : {
      create: function(args) {
        return new node(args.data, _this);
      }
      ,
      key: function(data) {
        return ko.utils.unwrapObservable(data.id);
      }
    }
  };

  ko.mapping.fromJS(config, mappingOptions, this);
}

$(function(){
  $.getJSON('data.json', function(data) {
    viewModel = new node(data, undefined);

    (function() {
      function flattenChildren(children, result) {
        ko.utils.arrayForEach(children(), function(child) {
          result.push(child);
          if (child.Children) {
            flattenChildren(child.Children, result);
          }
        });
      }

      viewModel.flattened = ko.dependentObservable(function() {
        var result = []; //root node

        if (viewModel.Children) {
          flattenChildren(viewModel.Children, result);   
        }

        return result;
      });

      viewModel.update = function () {
        return new Promise((resolve, reject) => {
          requestTimer = viewModel.timer;
          $.getJSON('data.json', function (data) {
            ko.mapping.fromJS(data, {}, viewModel);
          });
          // Autorefresh stopped or new autorefresh has been set.
          if (viewModel.timer == 0 || requestTimer != viewModel.timer)
            reject();
          else
            resolve();
        })
      }

      viewModel.rate = 3000; //milliseconds
      viewModel.timer = {};  // unique ID during page lifetime

      viewModel.startAuto = function (){
        viewModel.timer = setTimeout(function updateRequest() {
          viewModel.update().then(() => viewModel.timer = setTimeout(updateRequest, viewModel.rate));
        }, viewModel.rate);
      }

      viewModel.stopAuto = function () {
        clearTimeout(viewModel.timer);
        viewModel.timer = 0;
      }

      viewModel.auto_refresh = ko.observable(false);
      viewModel.toggleAuto = ko.dependentObservable(function() {
        if (viewModel.auto_refresh())
          viewModel.startAuto();
        else
          viewModel.stopAuto();
      }, viewModel);

    })();

    ko.applyBindings(viewModel);
    $("#tree").treeTable({
      initialState: "expanded",
      clickableNodeNames: true
    });
  });
  $( "#refresh" ).button();
  $( "#auto_refresh" ).button();
  $( "#slider" ).slider({
    value:3000,
    min: 100,
    max: 10000,
    step: 100,
    slide: function( event, ui ) {
      viewModel.rate = ui.value;
      if (viewModel.auto_refresh()) {
        //reset the timer
        viewModel.stopAuto();
        viewModel.startAuto();
      }
      $( "#lbl" ).text( ui.value + "ms");
    }
  });
  $( "#lbl" ).text( $( "#slider" ).slider( "value" ) + "ms");

});


