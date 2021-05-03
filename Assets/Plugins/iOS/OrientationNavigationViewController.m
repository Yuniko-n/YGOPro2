//
//  OrientationNavigationViewController.m
//  Unity-iPhone
//
//  Created by Piotr on 16/07/17.
//
//

#import "OrientationNavigationViewController.h"

@interface OrientationNavigationViewController ()

@end

@implementation OrientationNavigationViewController

- (UIInterfaceOrientationMask)supportedInterfaceOrientations {
    if (_shouldStickToPortrait) {
        return UIInterfaceOrientationMaskPortrait;
    } else if (_shouldStickToLandscape) {
        return UIInterfaceOrientationMaskLandscape;
    }
    
    return UIInterfaceOrientationMaskAll;
}

-(void)dismissViewControllerAnimated:(BOOL)flag completion:(void (^)(void))completion
{
    if ( self.presentedViewController)
    {
        [super dismissViewControllerAnimated:flag completion:completion];
    }
}


@end

@interface StandardNavigationController ()

@end

@implementation StandardNavigationController
-(void)dismissViewControllerAnimated:(BOOL)flag completion:(void (^)(void))completion
{
    if ( self.presentedViewController)
    {
        [super dismissViewControllerAnimated:flag completion:completion];
    }
}
@end
