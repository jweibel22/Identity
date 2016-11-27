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
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
                            }
                        },
                        'header': {
                            templateUrl: 'Content/templates/menu.html',
                            controller: 'NavigatorCtrl',
                            resolve: { userPromise: ['userService', function (userService) { return userService.userPromise.promise; }] }
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
                            templateUrl: 'Content/templates/channel.html',
                            controller: 'ChannelController',
                            resolve: {
                                channelPromise: ['$stateParams', 'channelService', function ($stateParams, channelService) { return channelService.getById($stateParams.channelId); }],
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
                            }
                        }
                    }
                })
                .state('root.home', {
                    url: '/home',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/home.html',
                            controller: 'HomeController',
                            resolve: {
                                homePromise: ['$stateParams', 'homeService', function ($stateParams, homeService) { return homeService.getHomeScreenContents(); }],
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
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
                .state('root.allchannels', {
                    url: '/allchannels',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/allChannels.html',
                            controller: 'AllChannelsController',
                            resolve: {
                                _: ['channelService', function (channelService) { return channelService.allPublic(); }]
                            }
                        }
                    }
                })
                .state('root.searchByTag', {
                    url: '/searchByTag?query',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/searchresults.html',
                            controller: 'SearchController',
                            resolve: {
                                posts: ['$stateParams', 'postService', function ($stateParams, postService) { return postService.getByTag($stateParams.query); }],
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
                            }
                        }
                    }
                })
                .state('root.search', {
                    url: '/searchChannelOrUser?query',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/searchresults.html',
                            controller: 'SearchController',
                            resolve: {
                                channels: ['$stateParams', 'channelService', function ($stateParams, channelService) { return channelService.findByName($stateParams.query); }],
                                users: ['$stateParams', 'userService', function ($stateParams, userService) { return userService.findByName($stateParams.query); }],
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
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
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }],
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
                            controller: 'EditChannelController',
                            resolve: {
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }],
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
                })
                .state('root.readhistory', {
                    url: '/readhistory',
                    views: {
                        'container@': {
                            templateUrl: 'Content/templates/readhistory.html',
                            controller: 'ReadHistoryController',
                            resolve: {
                                posts: ['$stateParams', 'postService', function ($stateParams, postService) { return postService.readHistory(); }],
                                userPromise: ['userService', function (userService) { return userService.userPromise.promise; }]
                            }
                        }
                    }
                });

            $urlRouterProvider.otherwise('/home');
        }
    ])

.run(['authService', 'userService', function (authService, userService) {
    authService.fillAuthData();
    var tmp = userService.getCurrentUser(); //this ensures that the user is loaded in the UserService
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

                if (response.status != 401) {
                    toasty.error({
                        title: 'Error',
                        msg: response.data.ExceptionMessage
                    });
                }

                return $q.reject(response);
            });
        };
    }])
;


