angular.module('inspire').factory('channelService', ['$http', 'ngSettings', function ($http, ngSettings) {

    var o = {
        channels: [], //subscribed channels
        editchannel: {},
        searchResult: []
    };

    o.allPublic = function() {
        return $http.get(ngSettings.baseUrl + '/Api/Channel').success(function(data){
            angular.copy(data, o.channels);
        });
    };

    o.subscribe = function(channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Subscribe");
    };

    o.unsubscribe = function(channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Unsubscribe");
    };

    o.leave = function (channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Leave");
    };

    o.create = function (channel) {
        return $http.post(ngSettings.baseUrl + '/Api/Channel/', channel).success(function (data) {
            o.channels.push(data);
        });
    };

    o.update = function (channel) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channel.Id + '/Update', channel).success(function (res) {
            
        });
    };

    o.delete = function (channel) {
        return $http.delete(ngSettings.baseUrl + '/Api/Channel/' + channel.Id).success(function (data) {
            var index = channels.indexOf(channel);
            if (index > -1) {
                channels.splice(index, 1);
            }
        });
    };

    o.getById = function(id) {
        return $http.get(ngSettings.baseUrl + '/Api/Channel/' + id).success(function (data) {
            o.editchannel = data;
        });
    }

    o.findByName = function(query) {
        return $http.get(ngSettings.baseUrl + '/Api/Channel?query=' + query).success(function (data) {
            angular.copy(data, o.searchResult);
        });
    }

    return o;
}]);
