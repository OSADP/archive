// usage: log('inside coolFunc', this, arguments);
// paulirish.com/2009/log-a-lightweight-wrapper-for-consolelog/
window.log = function f() {
    log.history = log.history || [];
    log.history.push(arguments);
    if (this.console) {
        var args = arguments, newarr;
        args.callee = args.callee.caller;
        newarr = [].slice.call(args);
        if (typeof console.log === 'object')
            log.apply.call(console.log, console, newarr);
        else
            console.log.apply(console, newarr);
    }
};

// make it safe to use console.log always
(function (a) {
    function b() {
    }
    for (var c = "assert,count,debug,dir,dirxml,error,exception,group,groupCollapsed,groupEnd,info,log,markTimeline,profile,profileEnd,time,timeEnd,trace,warn".split(","), d; !!(d = c.pop()); ) {
        a[d] = a[d] || b;
    }
})
        (function () {
            try {
                console.log();
                return window.console;
            } catch (a) {
                return (window.console = {});
            }
        }());

// source:
// http://filamentgroup.com/lab/jquery_plugin_for_requesting_ajax_like_file_downloads/
jQuery.download = function (url, data, method) {
    //url and data options required
    if (url && data) {
        //data can be string of parameters or array/object
        data = typeof data === 'string' ? data : jQuery.param(data);
        var form = jQuery('<form action="' + url + '" method="' + (method || 'post') + '"></form>');
        //split params into form inputs
        jQuery.each(data.split('&'), function () {
            var pair = this.split('=');
            form.append($('<input type="hidden" />').attr('name', pair[0]).attr('value', pair[1]));
        });
        //send request
        form.appendTo('body').submit().remove();
    }
};

// source: http://stackoverflow.com/a/5624139
function rgbToHex(r, g, b) {
    return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

/*
 * TODO: we should probably update this and the other one to just use a boolean param...
 */
function getBaseURI(port, contextPath) {
    var baseURL = location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + location.pathname;
    return baseURL;
}

function getBaseURIWithoutContextPath() {
    var baseURL = location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + '/';
    return baseURL;
}

// source: http://stackoverflow.com/a/1404100
function getURLParameter(name) {
    return decodeURI((RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, null])[1]);
}

function getParameterMap(loci) {
    var parameterMap = {};
    var query = loci.substring(loci.lastIndexOf('?') + 1);
    $.map(query.split('&'), function (pair) {
        var tokens = pair.split('=');
        parameterMap[tokens[0]] = tokens[1];
    });
    return parameterMap;
}

function getParameterMapString(loci) {
    return loci.substring(loci.lastIndexOf('?'));
}

// http://stackoverflow.com/questions/196972/convert-string-to-title-case-with-javascript
String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
};

// http://stackoverflow.com/questions/4253367/how-to-escape-a-json-string-containing-newline-characters-using-javascript
String.prototype.escapeSpecialChars = function () {
    return this.replace(/\\/g, "\\\\").replace(/\n/g, "\\n").replace(/\r/g, "\\r").replace(/\t/g, "\\t").replace(/\f/g, "\\f").replace(/"/g, "\\\"").replace(/'/g, "\\\'").replace(/\&/g, "\\&");
};

// should make a funcition that does the prototype for just the contensts of the object


// MDI tooltip function
function mdiToolTip() {
    var title = '';
    title += 'For Wildcard use %';
    title += '\n';
    title += 'To return all organisms only type  %';
    $('#posHomeMDISearchBox > input').attr('title', title);
    $('#posMDISmallSearchBox > input').attr('title', title); // yeah... we have two names for this...
    $('#queryDiv > input').attr('title', title); // yeah... make that three....
}

// http://stackoverflow.com/questions/5353934/check-if-element-is-visible-on-screen
function isOnScreen(elm) {
    var vpH = $(window).height(), // Viewport Height
            st = $(window).scrollTop(), // Scroll Top
            y = $(elm).offset().top;
    var visible = vpH + st;
    return (y > st && y <= visible);
}

// http://stackoverflow.com/a/196991
function toTitleCase(str) {
    return str.replace(/\w\S*/g, function (txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    });
}

function getAppName() {
    var path = window.location.pathname;
    return path.split("/")[1];
}

function setActiveBranch(branchId) {
    var branch = '#' + branchId;
    if (branchId !== undefined) {
        // let's wait 15 milliseconds before placing this... may be able to remove it...
        setTimeout(function () {
            $(branch).addClass('navigationActiveBranch');
        }, 15);
    }
}

// set the center section height correctly
function resizeMiddle() {
    // since they may not be done initiallizing before the resize function is called, will redo this here...
    //console.log('resizing: header-' + $('#header').height() + ' - footer-' + $('#footer').height());
    var h = $(window).height() - ($('#header').height() === 0 ? 150 : $('#header').height()) - (footerHeight = $('#footer').height() === 0 ? 100 : $('#footer').height());
    h = $(window).height() - 250;
    $('#data_panel').css('min-height', h);
}

function sortByKey(array, key, asc) {
    if (asc === undefined || asc) { // ascending (default)
        return array.sort(function (a, b) {
            var x = a[key];
            var y = b[key];
            return ((x < y) ? -1 : ((x > y) ? 1 : 0));
        });
    } else {    // do it descending!
        return array.sort(function (a, b) {
            var y = a[key];
            var x = b[key];
            return ((x < y) ? -1 : ((x > y) ? 1 : 0));
        });
    }
}

//http://stackoverflow.com/questions/1359761/sorting-a-javascript-object
function sortObject(o) {
    var sorted = {},
            key, a = [];
    for (key in o) {
        if (o.hasOwnProperty(key)) {
            a.push(key);
        }
    }
    a.sort();
    for (key = 0; key < a.length; key++) {
        sorted[a[key]] = o[a[key]];
    }
    return sorted;
}

//http://stackoverflow.com/questions/1359761/sorting-a-javascript-object
function sortObjectReverse(o) {
    var sorted = {},
            key, a = [];
    for (key in o) {
        if (o.hasOwnProperty(key)) {
            a.push(key);
        }
    }
    a.sort().reverse();
    for (key = 0; key < a.length; key++) {
        sorted[a[key]] = o[a[key]];
    }
    return sorted;
}

/**
 * http://stackoverflow.com/a/3149499
 *
 * Use this function as jQuery "load" to disable request caching in IE
 * Example: $('selector').loadWithoutCache('url', function(){ //success function callback... });
 **/
$.fn.loadWithoutCache = function () {
    var elem = $(this);
    var func = arguments[1];
    $.ajax({
        url: arguments[0],
        cache: false,
        dataType: "html",
        success: function (data, textStatus, XMLHttpRequest) {
            elem.html(data);
            if (func !== undefined) {
                func(data, textStatus, XMLHttpRequest);
            }
        }
    });
    return elem;
};

/*
 * function to load a given css file
 */
loadCSS = function (href) {
    var cssLink = $("<link rel='stylesheet' type='text/css' href='" + href + "'>");
    $("head").append(cssLink);
};

/*
 * function to load a given js file
 */
loadJS = function (src) {
    var jsLink = $("<script type='text/javascript' src='" + src + "'>");
    $("head").append(jsLink);
};

/*
 * http://stackoverflow.com/questions/3387427/javascript-remove-element-by-id/18120786#18120786
 */
Element.prototype.remove = function () {
    this.parentElement.removeChild(this);
};
NodeList.prototype.remove = HTMLCollection.prototype.remove = function () {
    for (var i = 0, len = this.length; i < len; i++) {
        if (this[i] && this[i].parentElement) {
            this[i].parentElement.removeChild(this[i]);
        }
    }
};
/*
 * http://www.codeproject.com/Articles/693841/Making-Dashboards-with-Dc-js-Part-Using-Crossfil
 */
function print_filter(filter) {
    var f = eval(filter);
    if (typeof (f.length) !== "undefined") {
    } else {
    }
    if (typeof (f.top) !== "undefined") {
        f = f.top(Infinity);
    } else {
    }
    if (typeof (f.dimension) !== "undefined") {
        f = f.dimension(function (d) {
            return "";
        }).top(Infinity);
    } else {
    }
    console.log(filter + "(" + f.length + ") = " + JSON.stringify(f).replace("[", "[\n\t").replace(/}\,/g, "},\n\t").replace("]", "\n]"));
}

/*
 * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/Trim
 */
if (!String.prototype.trim) {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

/*
 * http://stackoverflow.com/questions/6454198/check-a-range-of-numbers-in-an-if-condition
 */
function between(x, min, max) {
    return x >= min && x <= max;
}

function get_browser() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE ' + (tem[1] || '');
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/)
        if (tem != null) {
            return 'Opera ' + tem[1];
        }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    if ((tem = ua.match(/version\/(\d+)/i)) != null) {
        M.splice(1, 1, tem[1]);
    }
    return M[0];
}

function get_browser_version() {
    var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
    if (/trident/i.test(M[1])) {
        tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
        return 'IE ' + (tem[1] || '');
    }
    if (M[1] === 'Chrome') {
        tem = ua.match(/\bOPR\/(\d+)/);
        if (tem !== null) {
            return 'Opera ' + tem[1];
        }
    }
    M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
    tem = ua.match(/version\/(\d+)/i);
    if (tem !== null) {
        M.splice(1, 1, tem[1]);
    }
    return M[1];
}

function getAverageFromNumArr(numArr) {
    if (!$.isArray(numArr)) {
        return false;
    }
    var length = numArr.length;
    var i = length;
    var sum = 0;
    while (i--) {
        sum += numArr[i];
    }
    return (sum / length);
}
function getVariance(numArr) {
    if (!$.isArray(numArr)) {
        return false;
    }
    var avg = getAverageFromNumArr(numArr),
            i = numArr.length,
            v = 0;

    while (i--) {
        v += Math.pow((numArr[ i ] - avg), 2);
    }
    v /= numArr.length;
    return v;
}
function getStandardDeviation(numArr) {
    if (!$.isArray(numArr)) {
        return false;
    }
    var stdDev = Math.sqrt(getVariance(numArr));
    return stdDev;
}
function roundAccuracy(num, acc)
{
    var factor = Math.pow(10, acc);
    return (Math.round(num / factor) * factor);
}
function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
function getNumDigitsBeforeDecimal(num) {
    var num = num + '';
    var s = num.split('.');
    return s[0].length;
}

function formatMoney(number, places, symbol, thousand, decimal) {
    number = number || 0;
    places = !isNaN(places = Math.abs(places)) ? places : 2;
    symbol = symbol !== undefined ? symbol : "$";
    thousand = thousand || ",";
    decimal = decimal || ".";
    var negative = number < 0 ? "-" : "",
            i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
            j = (j = i.length) > 3 ? j % 3 : 0;
    return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
}

function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}