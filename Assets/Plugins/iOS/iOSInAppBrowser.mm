//
//  iOSInAppBrowser.m
//
//  Created by Piotr Zmudzinski on 04/03/16.
//  contact: ptr.zmudzinski@gmail.com
//

#import <Foundation/Foundation.h>
#import "UnityAppController.h"
#import "InAppBrowserViewController.h"
#import "OrientationNavigationViewController.h"

struct DisplayOptions {
    char *pageTitle;
    char *backButtonText;
    char *barBackgroundColor;
    char *textColor;
    char *browserBackgroundColor;
    char *loadingInidicatorColor;
    bool displayURLAsPageTitle;
    bool hidesTopBar;
    bool pinchAndZoomEnabled;
    bool shouldUsePlaybackCategory;
    bool shouldStickToPortrait;
    bool shouldStickToLandscape;
    bool androidBackButtonCustomBehaviour;
    bool mixedContentCompatibilityMode;
    bool webContentsDebuggingEnabled;
    bool shouldUseWideViewPort;
    bool hidesDefaultSpinner;
    bool hidesHistoryButtons;
    bool setLoadWithOverviewMode;
    
    char *historyButtonsFontSize;
    char *titleFontSize;
    char *titleLeftRightPadding;
    char *backButtonFontSize;
    char *backButtonLeftRightMargin;
};

NSString *stringFromChar(char* bytes) {
    if (bytes == NULL) {
        return nil;
    } else {
        return [NSString stringWithUTF8String:bytes];
    }
}

@interface UIColor(HexString)

+ (CGFloat) colorComponentFrom:(NSString *) string start:(NSUInteger) start length:(NSUInteger) length;

@end


@implementation UIColor(HexString)

+ (UIColor *) colorWithHexChar: (char *) hexChar {
    return [UIColor colorWithHexString:[NSString stringWithUTF8String:hexChar]];
}

+ (UIColor *) colorWithHexString: (NSString *) hexString {
    NSString *colorString = [[hexString stringByReplacingOccurrencesOfString: @"#" withString: @""] uppercaseString];
    CGFloat alpha, red, blue, green;
    switch ([colorString length]) {
        case 3: // #RGB
            alpha = 1.0f;
            red   = [self colorComponentFrom: colorString start: 0 length: 1];
            green = [self colorComponentFrom: colorString start: 1 length: 1];
            blue  = [self colorComponentFrom: colorString start: 2 length: 1];
            break;
        case 4: // #ARGB
            alpha = [self colorComponentFrom: colorString start: 0 length: 1];
            red   = [self colorComponentFrom: colorString start: 1 length: 1];
            green = [self colorComponentFrom: colorString start: 2 length: 1];
            blue  = [self colorComponentFrom: colorString start: 3 length: 1];
            break;
        case 6: // #RRGGBB
            alpha = 1.0f;
            red   = [self colorComponentFrom: colorString start: 0 length: 2];
            green = [self colorComponentFrom: colorString start: 2 length: 2];
            blue  = [self colorComponentFrom: colorString start: 4 length: 2];
            break;
        case 8: // #AARRGGBB
            alpha = [self colorComponentFrom: colorString start: 0 length: 2];
            red   = [self colorComponentFrom: colorString start: 2 length: 2];
            green = [self colorComponentFrom: colorString start: 4 length: 2];
            blue  = [self colorComponentFrom: colorString start: 6 length: 2];
            break;
        default:
            return nil;
    }
    return [UIColor colorWithRed: red green: green blue: blue alpha: alpha];
}

+ (CGFloat) colorComponentFrom: (NSString *) string start: (NSUInteger) start length: (NSUInteger) length {
    NSString *substring = [string substringWithRange: NSMakeRange(start, length)];
    NSString *fullHex = length == 2 ? substring : [NSString stringWithFormat: @"%@%@", substring, substring];
    unsigned hexComponent;
    [[NSScanner scannerWithString: fullHex] scanHexInt: &hexComponent];
    return hexComponent / 255.0;
}

@end


@interface InAppBrowserConfig(DisplayOptionsStruct)

+ (InAppBrowserConfig*)fromDisplayOptions:(struct DisplayOptions)options;

@end

@implementation InAppBrowserConfig(DisplayOptionsStruct)

+ (InAppBrowserConfig*)fromDisplayOptions:(struct DisplayOptions)options {
    InAppBrowserConfig *config = [InAppBrowserConfig defaultDisplayOptions];
    
    if (options.pageTitle != NULL) {
        config.pageTitle = [NSString stringWithUTF8String:options.pageTitle];
    }
    
    if (options.backButtonText != NULL) {
        config.backButtonText = [NSString stringWithUTF8String:options.backButtonText];
    }
    
    if (options.barBackgroundColor != NULL) {
        config.barBackgroundColor = [UIColor colorWithHexChar:options.barBackgroundColor];
    }
    
    if (options.textColor != NULL) {
        config.textColor = [UIColor colorWithHexChar:options.textColor];
    }
    
    if (options.browserBackgroundColor != NULL) {
        config.browserBackgroundColor = [UIColor colorWithHexChar:options.browserBackgroundColor];
    }
    
    if (options.loadingInidicatorColor != NULL) {
        config.loadingIndicatorColor = [UIColor colorWithHexChar:options.loadingInidicatorColor];
    }
    
    config.displayURLAsPageTitle = options.displayURLAsPageTitle;
    config.hidesTopBar = options.hidesTopBar;
    config.pinchAndZoomEnabled = options.pinchAndZoomEnabled;
    config.shouldUsePlaybackCategory = options.shouldUsePlaybackCategory;
    
    config.titleFontSize = stringFromChar(options.titleFontSize);
    config.titleLeftRightPadding = stringFromChar(options.titleLeftRightPadding);
    config.backButtonFontSize = stringFromChar(options.backButtonFontSize);
    config.backButtonLeftRightMargin = stringFromChar(options.backButtonLeftRightMargin);
    
    config.shouldStickToPortrait = options.shouldStickToPortrait;
    config.shouldStickToLandscape = options.shouldStickToLandscape;
    
    config.hidesHistoryButtons = options.hidesHistoryButtons;
    return config;
}

@end

InAppBrowserViewController *GetInAppBrowserViewController() {
    UnityAppController *unityAppController = GetAppController();
    if ([unityAppController.rootViewController.presentedViewController isKindOfClass:[UINavigationController class]]) {
        
        UINavigationController *presentedNavVC = (UINavigationController *)unityAppController.rootViewController.presentedViewController;
        
        if ([presentedNavVC.viewControllers[0] isKindOfClass:[InAppBrowserViewController class]]){
            return (InAppBrowserViewController *)presentedNavVC.viewControllers[0];
        }
    }
    return nil;
}


extern "C" {
    
    BOOL _CanGoForward() {
        InAppBrowserViewController *browserVC = GetInAppBrowserViewController();
        return [browserVC canGoForward];
    }
    
    BOOL _CanGoBack() {
        InAppBrowserViewController *browserVC = GetInAppBrowserViewController();
        return [browserVC canGoBack];
    }
    
    void _GoForward() {
        InAppBrowserViewController *browserVC = GetInAppBrowserViewController();
        [browserVC goForward];
    }
    
    void _GoBack() {
        InAppBrowserViewController *browserVC = GetInAppBrowserViewController();
        [browserVC goBack];
    }
    
    BOOL _IsInAppBrowserOpened() {
        UnityAppController *unityAppController = GetAppController();
        if ([unityAppController.rootViewController.presentedViewController isKindOfClass:[UINavigationController class]]) {
            
            UINavigationController *presentedNavVC = (UINavigationController *)unityAppController.rootViewController.presentedViewController;
            
            if ([presentedNavVC.viewControllers[0] isKindOfClass:[InAppBrowserViewController class]]){
                return YES;
            }
        }
        
        return NO;
    }
    
    void _PresentInAppBrowserViewController(InAppBrowserViewController *vc) {
        UnityAppController *unityAppController = GetAppController();
    
        InAppBrowserConfig *config = vc.config;
        UINavigationController *navigationController;
        if (config.shouldStickToLandscape || config.shouldStickToPortrait) {
            navigationController = [[OrientationNavigationViewController alloc] initWithRootViewController:vc];
            OrientationNavigationViewController *orientationVC = (OrientationNavigationViewController *)navigationController;
            orientationVC.shouldStickToLandscape = config.shouldStickToLandscape;
            orientationVC.shouldStickToPortrait = config.shouldStickToPortrait;
        } else {
            navigationController = [[StandardNavigationController alloc] initWithRootViewController:vc];
        }
        
        
        [unityAppController.rootViewController presentViewController:navigationController animated:true completion:nil];
    }
    
    void _ShowURLWithDisplayOptions(NSString *urlAsString, DisplayOptions displayOptions) {
        InAppBrowserViewController *vc = [InAppBrowserViewController new];
        vc.URL = urlAsString;
        vc.config = [InAppBrowserConfig fromDisplayOptions: displayOptions];
        _PresentInAppBrowserViewController(vc);
    }
    
    void _ShowHTMLWithDisplayOptions(NSString *HTMLAsString, DisplayOptions displayOptions) {
        InAppBrowserViewController *vc = [InAppBrowserViewController new];
        vc.HTML = HTMLAsString;
        vc.config = [InAppBrowserConfig fromDisplayOptions: displayOptions];
        _PresentInAppBrowserViewController(vc);
    }
    
    void _OpenInAppBrowser(char *URL, struct DisplayOptions displayOptions) {
        NSString *urlAsString = [NSString stringWithUTF8String:URL];
        _ShowURLWithDisplayOptions(urlAsString, displayOptions);
    }
    
    void _LoadHTML(char *HTML, struct DisplayOptions displayOptions) {
        NSString *HTMLAsString = [NSString stringWithUTF8String: HTML];
        _ShowHTMLWithDisplayOptions(HTMLAsString, displayOptions);
    }
    
    void _CloseInAppBrowser() {
        UnityAppController *unityAppController = GetAppController();
        if ([unityAppController.rootViewController.presentedViewController isKindOfClass:[UINavigationController class]]) {
            
            UINavigationController *presentedNavVC = (UINavigationController *)unityAppController.rootViewController.presentedViewController;
            
            if ([presentedNavVC.viewControllers[0] isKindOfClass:[InAppBrowserViewController class]]){
                [unityAppController.rootViewController dismissViewControllerAnimated:true completion:nil];
            }
        }
    }
    
    void _ClearCache() {
        [[NSURLCache sharedURLCache] removeAllCachedResponses];
        NSHTTPCookieStorage *cookieStorage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
        NSArray<NSHTTPCookie *> *cookies = cookieStorage.cookies;
        for (NSHTTPCookie *cookie in cookies) {
            [cookieStorage deleteCookie:cookie];
        }
        [[NSUserDefaults standardUserDefaults] synchronize];
    }
    
    void _SendJSMessage(char *message) {
        NSString *messageString = [NSString stringWithUTF8String:message];
        InAppBrowserViewController *vc = GetInAppBrowserViewController();
        if (vc != nil) {
            [vc sendJSMessage:messageString];
        }
    }
    
}

