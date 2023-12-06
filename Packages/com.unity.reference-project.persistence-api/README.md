# Replica - Persistence API 

This sample will showcase how the Persistence API can be used to script a Google Drive API client to be used for both local storage and cloud storage.

Input your token in the field and press "Send Screenshot".

Your screenshot will appear in the Result window, with the location of the screenshot shown below it.

You can click the "Open Screenshot" button to view saved screenshot.

The local path matches the url path allowing all of the same client requests to be called, whether the device is offline/online or you just want to quarantine all the data to a local device.
No need to write an entire new offline client, just change the base url when initializing the Persistence API and business as usual.