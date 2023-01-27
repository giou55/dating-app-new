import { Injectable } from '@angular/core';
//import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { CanActivate } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  // remember that the route guards automatically subscribe and unsubscribe to observables,
  // so we don't need to subscribe to this

  constructor(private accountService: AccountService, private toastr: ToastrService) {}

  canActivate(
    // use these if you want access to the route information
    // route: ActivatedRouteSnapshot,
    // state: RouterStateSnapshot
    ): Observable<boolean> {
      return this.accountService.currentUser$.pipe(
        map(user => {
          // obviously he is not an admin, if he is not even logged in
          if (!user) return false;

          if (user.roles.includes('Admin') || user.roles.includes('Moderator')) {
            return true;
          } else {
            this.toastr.error('You cannot enter this area');
            return false;
          }
        })
      )
  }
  
}
