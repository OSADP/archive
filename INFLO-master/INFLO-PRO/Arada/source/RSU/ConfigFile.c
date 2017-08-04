#include "ConfigFile.h"

#include <stdio.h>
#include <stdbool.h>

#define LINE_BUFFER_SIZE 1024
#define KEY_BUFFER_SIZE 512
#define VALUE_BUFFER_SIZE 512

static ConfigFile* gpConfigFile = NULL;

static const char* TIM_GET_URL                  = "TimGetUrl";
static const char* BSM_POST_URL                  = "BsmPostUrl";

char* trimWhitespace(char* str)
{
    char *end;

    // Trim leading space
    while(isspace(*str)) str++;

    if(*str == 0)  // All spaces?
    return str;

    // Trim trailing space
    end = str + strlen(str) - 1;
    while(end > str && isspace(*end)) end--;

    // Write new null terminator
    *(end+1) = 0;

    return str;
}

void cfParseLine(char* pLine)
{
    pLine = trimWhitespace(pLine);

    int separatorPos = 0;
    int lineLength = strlen(pLine);
    
    if(lineLength < 3)
    {
        return;
    }
    
    if(pLine[0] == '#')
    {
        return;
    }
    
    while(separatorPos != lineLength && pLine[separatorPos] != '=')
    {
        separatorPos++;
    }    
    pLine[separatorPos] = '\0';
    
    char* key = trimWhitespace(pLine);
    char* value = trimWhitespace(&pLine[separatorPos+1]);
    bool success = true;
    
    printf("Cfg: k=%s, v=%s\n", key, value);
    
    if(strcmp(key, TIM_GET_URL) == 0)
    {
        strcpy(gpConfigFile->tim_get_url, value);
    }
    else if(strcmp(key, BSM_POST_URL) == 0)
    {
        strcpy(gpConfigFile->bsm_post_url, value);
    }
    else
    {
        printf("ConfigFile: Unknown field %s\n", key);
    }
    
    if(!success)
    {        
        printf("Error parsing %s, %s\n", key, value);
    }
    
    return;
}

void cfSetDefaults()
{
    
    return;    
}

int cfInitialize(const char* pFilename)
{
    gpConfigFile = (ConfigFile*)malloc(sizeof(ConfigFile));
    
    cfSetDefaults();
    
    FILE* pFile = fopen(pFilename, "r");
    
    if(!pFile)
    {
        return false;
    }
    
    char buffer[LINE_BUFFER_SIZE];
    while(fgets(buffer, LINE_BUFFER_SIZE, pFile))
    {
        cfParseLine(buffer);
    }
    
    fclose(pFile);
    
    return true;
}

ConfigFile* cfGetConfigFile()
{
    return gpConfigFile;
}
