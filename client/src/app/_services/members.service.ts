import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  // we store members here, so they will be available to every component
  // without making a new request to database (caching data)
  members: Member[] = [];

  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>;

  constructor(private http:HttpClient) { }

  getMembers(page?: number, itemsPerPage?: number) {
    let params = new HttpParams();

    if (page && itemsPerPage) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    // we turned off the caching of members
    //if (this.members.length > 0) return of(this.members);

    // we want to observe the response, and not just the body
    // and also we want to pass the params
    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).pipe(
      map(response => {
        if (response.body) {
          this.paginatedResult.result = response.body;        
        }
        const pagination = response.headers.get('Pagination');
        if (pagination) {
          this.paginatedResult.pagination = JSON.parse(pagination);        
        }
        // we return because the component need this
        return this.paginatedResult;
      })
    )
  }

  getMember(username: string) {
    const member = this.members.find(x => x.userName === username);

    // if member exists inside members array, return the member without making a request
    if (member) return of(member);

    //return this.http.get<Member>(this.baseUrl + 'users/' + username, this.getHttpOptions())
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  // we do not need this because we use jwt.interceptor to add Authorization header
  // getHttpOptions( ){
  //   const userString = localStorage.getItem('user');
  //   if (!userString) return;
  //   const user = JSON.parse(userString);
  //   return {
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + user.token
  //     })
  //   }
  // }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // nothing to return
      map(() => {
        const index = this.members.indexOf(member);
        // we update this specific member inside members array
        this.members[index] = {...this.members[index], ...member};
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
