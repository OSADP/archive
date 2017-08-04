// Map Customizations
window.trafficLayer = new google.maps.TrafficLayer();
window.trafficLayer.setMap(map);

furtherSetup();

map.mapTypes.set('usroadatlas', usRoadMapType);
map.setMapTypeId('usroadatlas');

google.maps.event.addListener(map, 'idle', mapChanged);

        function Tooltip(options) {
            this.marker_ = options.marker;
            this.content_ = options.content;
            this.map_ = options.marker.get('map');
            this.cssClass_ = options.cssClass||null;
        	this.id_ = options.id;
            this.div_ = null;
            this.setMap(this.map_);
            this.citizenfeedback = options.citizenfeedback;
            this.marker_.tooltip_ = this;
            
            var me = this;
            
            google.maps.event.addListener(me.marker_, 'click', function() 
            {
            	if(window.openTooltip)
                {
                	if(window.openTooltip.ddiv_)
                    {
                		window.openTooltip.ddiv_.infoOpen = false;
						$(window.openTooltip.ddiv_.closeBox).hide();
                    }
                	if(window.openTooltip.closeBox)
                    	$(window.openTooltip.closeBox).hide();
                        
                	$(window.openTooltip).slideUp();
                }
            	window.openTooltip = me.div_;
                me.show();
            });
        }
        Tooltip.prototype = new google.maps.OverlayView();
        Tooltip.prototype.onAdd = function() {
            var div = document.createElement('DIV');
            div.style.position = "absolute";
            div.style.width = "200px";
            div.style.height = "170px";
            div.style.overflowY = "scroll";
            div.id = this.id_;
            if(this.cssClass_)
                div.className += " "+this.cssClass_ + " ui-corner-all ui-widget-content";
                
            div.innerHTML = this.content_;
            div.style.cssText += "";
            div.closeBox = document.createElement('DIV');
            div.closeBox.style.zIndex = 5010;
            div.closeBox.style.cssText = div.closeBox.style.cssText + "cursor: arrow; padding:5px;";
            div.closeBox.ddiv = div;
            div.closeBox.innerHTML = '<div style=""><img src="images/icon-close.png"></div>';
            
            touchScroll(div);

	      	google.maps.event.addDomListener(div, 'mousedown', function(e) { e.stopPropagation(); });
	      	google.maps.event.addDomListener(div, 'dblclick', function(e) { e.stopPropagation(); });
	      	google.maps.event.addDomListener(div, 'DOMMouseScroll', function(e) { e.stopPropagation(); });
	      	google.maps.event.addDomListener(div, 'mousewheel', function(e) { e.stopPropagation(); });
            
            $(div.closeBox).click(function() 
            {
                $(this.ddiv).slideUp();
                $(this).hide();
            });
           
            this.div_ = div;
            $(this.div_).hide();
            $(this.div_.closeBox).hide();
            
            var panes = this.getPanes();
            panes.floatPane.appendChild(this.div_.closeBox);
            panes.floatPane.appendChild(this.div_);
            
            if(this.citizenfeedback)
            {
                $(this.citizenfeedback).button().click(function(){
					if(!window.isPhone)
					{
						var data = "<iframe width='400' height='475' src='reportpage.php?report=business-citizen.xml&cbid=" + $(this).attr('id').replace('citizenfeedback', '') + "'></iframe>";
						$('#userDialog').html( data );
						$( '#userDialog' ).dialog("open");
						$( '#userDialog' ).dialog('option', 'title', 'Citizen Business Update');
					}
					else
					{
						var data = "<iframe width='100%' height='100%' frameborder='0' src='reportpage.php?report=business-citizen.xml&cbid=" + $(this).attr('id').replace('citizenfeedback', '') + "&user=" + window.loginUser + "'></iframe>";
			            $('body').append('<div id="phoneReport" style="position:absolute;top:0;left:0;width:100%;height:100%;z-index:100004;background:#ffffff;overflow:hidden">' + data + '</div>');
			            $('#phoneReport').niceScroll({horizrailenabled:false, autohidemode:false});
					}
					
                });
            }
            
          }
        Tooltip.prototype.draw = function() 
        {
            var overlayProjection = this.getProjection();
            var ne = overlayProjection.fromLatLngToDivPixel(this.marker_.getPosition());
            // Position the DIV.
            var div = this.div_;
            div.style.left = ne.x + 'px';
            div.style.top = ne.y + 'px';
			div.closeBox.style.position = "absolute";	
			div.closeBox.style.top = ne.y - 18 + "px";
			div.closeBox.style.left = ne.x + $(div).width() - 8 +"px";
            
        }
        Tooltip.prototype.onRemove = function() 
        {
            this.div_.parentNode.removeChild(this.div_);
        }
        
        Tooltip.prototype.hide = function() 
        {
            if (this.div_) {
              $(this.div_).slideUp();
            }
        }
        
        Tooltip.prototype.show = function() 
        {
            if (this.div_) {
              $(this.div_).slideDown();
              $(this.div_.closeBox).show();
            }
        }