angular.module('inspire').directive("scroll", function ($window) {

    function checkVisible(elm) {
        var vpH = $(window).height(), // Viewport Height
        st = $(window).scrollTop(), // Scroll Top
        y = $(elm).offset().top,
        elementHeight = $(elm).height();

        var res = ((y < (vpH + st)) && (st < (y + elementHeight)));
        return res;
    };

    function aboveWindow(top, elm) {
        st = $(window).scrollTop(), // Scroll Top
        y = $(elm).offset().top;
        elementHeight = $(elm).height();

        var res = (y+elementHeight) < st + top;

        return res;
    };

    return function (scope, element, attrs) {

        var onScrollAction = function () {
            var divs = $(element).find('div.readable');

            var readPosts = [];

            for (var i = 0; i < divs.length; i++) {
                var top = 150; //TODO: this corresponds to the height of Menu+ChannelName+ChannelButtonMenu. (which corresponds to the place where an article goes out of sight) Replace this constant with a calculated value.  //$(element).offset().top
                if (aboveWindow(top, divs[i])) {
                    readPosts.push(scope.visiblePosts()[i]);
                }
            }

            if (readPosts.length > 0) {
                scope.read(readPosts);
            }


            scope.$apply();
        };

        angular.element($window).on('scroll', onScrollAction);

        scope.$on('$destroy', function () {
            angular.element($window).off('scroll', onScrollAction);
        });
    };
});