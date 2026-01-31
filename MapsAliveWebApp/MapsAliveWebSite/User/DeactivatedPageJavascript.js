document.writeln('<script type="text/javascript">');
document.writeln('if(typeof window.addEventListener!="undefined")');
document.writeln('window.removeEventListener("load",maEndLoad,false);');
document.writeln('else');
document.writeln('window.detachEvent("onload",maEndLoad);');
document.writeln('document.writeln("<iframe src=\'{0}ExpiredTour.ashx?maTourId={1}&maRef=" + document.referrer + "\' frameborder=0 scrolling=no width={2}px height={3}px ></iframe>");');
document.writeln('</script>');
