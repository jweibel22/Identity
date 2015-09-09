angular.module('inspire', ['ui.router', 'ui.bootstrap', 'ngSanitize', 'angular-jqcloud', 'infinite-scroll', 'LocalStorageModule', 'angular-toasty'])
    .constant('ngSettings', {
        //baseUrl: "http://localhost:8011",
        //baseUrl: "http://localhost:57294/",
		baseUrl: "http://inspireserver.azurewebsites.net/",	
        clientId: 'ngAuthApp'
    })
    .config(function($httpProvider) {
        $httpProvider.interceptors.push('authInterceptorService');
    })
    .config([
        '$stateProvider',
        '$urlRouterProvider',
        function($stateProvider, $urlRouterProvider) {

            $stateProvider
                .state('root', {
                    url: '',
                    abstract: true,
                    views: {
                        'channels': {
                            templateUrl: 'Content/templates/channels.html',
                            controller: 'ChannelsController',
                            resolve: {
                                userPromise: ['userService', function (userService) { return userService.getCurrentUser(); }]
                            }
                        },
                        'header': {
                            templateUrl: 'Content/templates/menu.html',
                            controller: 'NavigatorCtrl',
                            resolve: { userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }] }
                        }
                    }
                })
                .state('login', {
                    url: '/login',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/login.html',
                            controller: 'LoginController',
                            resolve: {

                            }
                        }
                    }
                })
                .state('associate', {
                    url: '/associate',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/associate.html',
                            controller: 'AssociateController',
                            resolve: {

                            }
                        }
                    }
                })
                .state('root.channel', {
                    url: '/home/{channelId}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/home.html',
                            controller: 'MainCtrl',
                            resolve: {
                                channelPromise: ['$stateParams', 'postService', function($stateParams, postService) { return postService.getFromChannel($stateParams.channelId, true, 'Added'); }],
                                userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }]
                            }
                        }
                    }
                })
                .state('root.home', {
                    url: '/home',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/home.html',
                            controller: 'MainCtrl',
                            resolve: {
                                channelPromise: ['$stateParams', 'postService', function($stateParams, postService) { return postService.getFromDefaultChannel(); }],
                                userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }]
                            }
                        }
                    }
                })
                .state('root.channels', {
                    url: '/channels',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/channels.html',
                            controller: 'ChannelsController',
                            resolve: {
                                _: ['channelService', function(channelService) { return channelService.allPublic(); }]
                            }
                        }
                    }
                })
                .state('root.archive', {
                    url: '/archive',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/archive.html',
                            controller: 'MainCtrl',
                            resolve: { _: ['postService', function(postService) { return postService.getAll(); }] }
                        }
                    }
                })
                .state('root.feed', {
                    url: '/feed',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/feed.html',
                            controller: 'FeedController',
                            resolve: {
                                posts: ['feedService', function(feedService) { return feedService.getFeed('Added'); }],
                                userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }]
                            }
                        }
                    }
                })
                .state('root.search', {
                    url: '/search?query',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/searchresults.html',
                            controller: 'SearchController',
                            resolve: {
                                posts: ['$stateParams', 'postService', function($stateParams, postService) { return postService.getByTag($stateParams.query) }],
                                userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }]
                            }
                        }
                    }
                })
                .state('root.channelsearch', {
                    url: '/channelsearch?query',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/channelSearchResults.html',
                            controller: 'ChannelSearchResultsController',
                            resolve: {
                                _: ['$stateParams', 'channelService', function($stateParams, channelService) { return channelService.findByName($stateParams.query); }]
                            }
                        }
                    }
                })
                .state('root.usersearch', {
                    url: '/usersearch?query',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/userSearchResults.html',
                            controller: 'UserSearchResultsController',
                            resolve: {
                                _: ['$stateParams', 'userService', function ($stateParams, userService) { return userService.findByName($stateParams.query); }]
                            }
                        }
                    }
                })
                .state('root.viewpost', {
                    url: '/viewpost/{id}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/viewpost.html',
                            controller: 'PostsCtrl',
                            resolve: {
                                post: [
                                    '$stateParams', 'postService', function($stateParams, postService) {
                                        return postService.get($stateParams.id);
                                    }
                                ]
                            }
                        }
                    }
                })
                .state('root.profile', {
                    url: '/profile/{id}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/profile.html',
                            controller: 'ProfileController',
                            resolve: {
                                userPromise: ['userService', function (userService) { return userService.getCurrentUser(); }],
                                profilePromise: ['$stateParams', 'userService', function ($stateParams, userService) { return userService.getUser($stateParams.id); }],
                                _: ['channelService', function(channelService) { return channelService.allPublic(); }]
                            }
                        }
                    }
                })
                .state('root.editpost', {
                    url: '/editpost/{id}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/editpost.html',
                            controller: 'PostsCtrl',
                            resolve: {
                                post: [
                                    '$stateParams', 'postService', function($stateParams, postService) {
                                        return postService.get($stateParams.id);
                                    }
                                ]
                            }
                        }
                    }
                })
                .state('root.editchannel', {
                    url: '/editchannel/{id}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/editchannel.html',
                            controller: 'ChannelController',
                            resolve: {
                                userPromise: ['userService', function(userService) { return userService.getCurrentUser(); }],
                                _: ['$stateParams', 'channelService', function($stateParams, channelService) { return channelService.getById($stateParams.id); }]
                            }
                        }
                    }
                })
                .state('root.editrssFeeder', {
                    url: '/editrssfeeder/{id}',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/editrssfeeder.html',
                            controller: 'RssFeederController',
                            resolve: {
                                rssFeederPromise: ['$stateParams', 'rssFeederService', function($stateParams, rssFeederService) { return rssFeederService.getById($stateParams.id); }]
                            }
                        }
                    }
                });

            $urlRouterProvider.otherwise('/home');
        }
    ])

.run(['authService', function (authService) {
    authService.fillAuthData();
}])
    .config(function ($httpProvider) {
        $httpProvider.responseInterceptors.push('myHttpInterceptor');
        var spinnerFunction = function (data, headersGetter) {
            // todo start the spinner here
            $('#loading').show();
            return data;
        };
        $httpProvider.defaults.transformRequest.push(spinnerFunction);
    })
    .factory('myHttpInterceptor', ['$q', 'toasty' , function ($q, toasty) {
        return function (promise) {
            return promise.then(function (response) {
                // do something on success
                // todo hide the spinner
                $('#loading').hide();
                return response;

            }, function (response) {
                $('#loading').hide();

                toasty.error({
                    title: 'Error',
                    msg: response.data.ExceptionMessage
                });

                return $q.reject(response);
            });
        };
    }])
;


