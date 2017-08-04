
function padToTwo(number) {
    if (number <= 9999) { number = ("000" + number).slice(-2); }
    return number;
}
function deMilitarize(number) {
    if (number > 12) { number = number - 12; }
    return number;
}