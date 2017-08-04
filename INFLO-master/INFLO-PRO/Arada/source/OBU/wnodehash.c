#include  <stdio.h>
#include  <stdint.h>
#include "wnodehash.h"


// return free widx
uint8_t ParseFreeWnode(struct wnode* wnodelist)
{
int i=0;
	for(i=1;i<MAX_LIST;i++) 
		if(wnodelist[i].validationstatus == 0) 
			return i; 
}

// return wnode idx 
uint8_t AllocateWnode(struct wnode* wnodelist, uint8_t vsts,  uint8_t maddr[])
{
uint8_t widx;
	
	widx=ParseFreeWnode(wnodelist);
	wnodelist[widx].validationstatus = vsts;
	memcpy(wnodelist[widx].macaddr,maddr,IEEE80211_ADDR_LEN);
	wnodelist[widx].nextidx=0;
	return widx;
}


// return hash idx uint8_t 
int8_t  LookupHash(struct wnodehash* whlist,struct wnode* wnodelist, uint8_t* maddr) 
{
uint8_t hashVal=0,hasdIdx=0,wIdx=0;
uint32_t macVal=0;

	memcpy(&macVal,maddr+2,sizeof(uint32_t));
	hashVal=(macVal%MAX_HASH_SIZE); //Calculating Hash Index
	wIdx=whlist[hashVal].head;

	if(wIdx == 0)
		return -1;	//No Hash entry Presents
        	
	else {	
		while(1) {
		if(!memcmp(wnodelist[wIdx].macaddr,maddr,IEEE80211_ADDR_LEN))  //Traversing the Hash Bucket list
			return hashVal;
		else if (wnodelist[wIdx].nextidx != 0)
			wIdx=wnodelist[wIdx].nextidx;
		else 	
			return -1;
	     	}
            }
	
}

// return status
uint8_t InsertHash(struct wnodehash* whlist, struct wnode* wnodelist, uint8_t widx, uint8_t hidx) 
{
uint8_t tail=0,head=0,hashVal=0,hasdIdx=0;
uint32_t macVal=0;
	
	memcpy(&macVal,wnodelist[widx].macaddr+2,sizeof(uint32_t));
	hashVal = (macVal%(MAX_HASH_SIZE)) ; //Calculating Hash Index
	head = whlist[hashVal].head;    
	tail = whlist[hashVal].tail;    
	
	if (head == 0)   
           {		                    //Adding new hash entry 
		whlist[hashVal].head=widx;
		whlist[hashVal].tail=0;
           }
	else if(tail == 0)   
	   {
		wnodelist[head].nextidx=widx; //Assigning next idx value to cur idx of  prev node
		whlist[hashVal].tail=widx;
	   }     	
	else 
	   {	
		wnodelist[tail].nextidx=widx;  //Adding to end of list
		whlist[hashVal].tail=widx;
	   }
		return 1;
}


//reset hast structures to  0 for  every five mins
void  FreeHash(struct wnodehash* whlist, struct wnode* wnodelist)
{
	memset(whlist, 0 ,MAX_HASH_SIZE * sizeof (struct wnodehash)); 
	memset(wnodelist, 0 ,MAX_LIST * sizeof (struct wnode));

}
