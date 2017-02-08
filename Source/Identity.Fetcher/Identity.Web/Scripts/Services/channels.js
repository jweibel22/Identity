angular.module('inspire').factory('channelService', ['$http', '$q', 'ngSettings', function ($http, $q, ngSettings) {

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

    o.rss = function (channelId) {
        return $http.get(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Rss");
    };

    o.subscribe = function(channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Subscribe");
    };

    o.unsubscribe = function(channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Unsubscribe");
    };

    o.markAllAsRead = function (channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/MarkAllAsRead");
    };

    o.leave = function (channelId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Leave");
    };

    o.grant = function (channelId, userId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + "/Grant?userId="+userId);
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

    o.updateDisplaySettings = function (channelId, userId, settings) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/UpdateDisplaySettings?userId='+userId, settings).success(function (res) {

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
        return $http.get(ngSettings.baseUrl + '/Api/Channel/' + id + "/GetById").success(function (data) {
            o.editchannel = data;
        });
    }

    o.findByName = function(query) {
        return $http.get(ngSettings.baseUrl + '/Api/Channel?query=' + query).success(function (data) {
            angular.copy(data, o.searchResult);
        });
    }

    o.removeSubscription = function (channel, child) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channel.Id + '/RemoveSubscription?childId=' + child.Id);
    };

    o.addSubscription = function (channelId, childId) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/AddSubscription?childId=' + childId);
    };

    o.addFeed = function (channelId, url, type) {
        return $http.put(ngSettings.baseUrl + '/Api/Channel/' + channelId + '/AddFeed?url=' + url + '&type=' + type);
    };

    o.getSubscriptions = function (id) {

        var deferred = $q.defer();

        $http.get(ngSettings.baseUrl + '/Api/Channel/' + id + "/GetById").success(function (data) {

            deferred.resolve({ data: data.Subscriptions });
        });

        return deferred.promise;
    };

    return o;
}]);
